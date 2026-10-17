from __future__ import annotations

from dataclasses import dataclass

import torch
from torch import Tensor, nn

from .config import PrismConfig


@dataclass
class VisualTokens:
    fused: Tensor
    coordinates: Tensor
    raw_channels: Tensor
    predicted_geometry: Tensor
    permutation: Tensor
    grid_size: tuple[int, int]


class PatchEncoder(nn.Module):
    def __init__(self, in_channels: int, out_dim: int, patch_size: int) -> None:
        super().__init__()
        self.projection = nn.Conv2d(
            in_channels, out_dim, kernel_size=patch_size, stride=patch_size
        )
        self.normalization = nn.LayerNorm(out_dim)

    def forward(self, x: Tensor) -> Tensor:
        x = self.projection(x).flatten(2).transpose(1, 2)
        return self.normalization(x)


class GeometryEstimator(nn.Module):
    """Small placeholder for a depth/normal/confidence foundation model."""

    def __init__(self, image_channels: int, geometry_channels: int) -> None:
        super().__init__()
        hidden = max(16, geometry_channels * 4)
        self.network = nn.Sequential(
            nn.Conv2d(image_channels, hidden, 3, padding=1),
            nn.GELU(),
            nn.Conv2d(hidden, hidden, 3, padding=1),
            nn.GELU(),
            nn.Conv2d(hidden, geometry_channels, 1),
        )

    def forward(self, images: Tensor) -> Tensor:
        return self.network(images)


class SemanticFlowReorder(nn.Module):
    """Ranks patches by learned saliency while preserving an inverse permutation."""

    def __init__(self, dim: int) -> None:
        super().__init__()
        self.score = nn.Sequential(nn.LayerNorm(dim), nn.Linear(dim, 1))

    def forward(self, tokens: Tensor) -> tuple[Tensor, Tensor]:
        scores = self.score(tokens).squeeze(-1)
        permutation = scores.argsort(dim=1, descending=True)
        gather_index = permutation.unsqueeze(-1).expand_as(tokens)
        return tokens.gather(1, gather_index), permutation


class UnifiedVisualEncoder(nn.Module):
    def __init__(self, config: PrismConfig) -> None:
        super().__init__()
        p = config.patch_size
        self.config = config
        self.semantic = PatchEncoder(config.image_channels, config.semantic_dim, p)
        self.appearance = PatchEncoder(config.image_channels, config.appearance_dim, p)
        self.geometry_estimator = GeometryEstimator(
            config.image_channels, config.geometry_channels
        )
        self.geometry = PatchEncoder(
            config.geometry_channels, config.geometry_dim, p
        )
        self.fusion = nn.Sequential(
            nn.LayerNorm(config.raw_token_dim),
            nn.Linear(config.raw_token_dim, config.model_dim),
        )
        self.reorder = SemanticFlowReorder(config.model_dim)

    @staticmethod
    def _coordinates(
        batch: int, height: int, width: int, device: torch.device, dtype: torch.dtype
    ) -> Tensor:
        y = torch.linspace(-1.0, 1.0, height, device=device, dtype=dtype)
        x = torch.linspace(-1.0, 1.0, width, device=device, dtype=dtype)
        yy, xx = torch.meshgrid(y, x, indexing="ij")
        coordinates = torch.stack((xx, yy), dim=-1).reshape(1, height * width, 2)
        return coordinates.expand(batch, -1, -1)

    def forward(self, images: Tensor, geometry: Tensor | None = None) -> VisualTokens:
        if images.ndim != 4:
            raise ValueError("images must have shape [batch, channels, height, width]")
        p = self.config.patch_size
        height, width = images.shape[-2:]
        if height % p or width % p:
            raise ValueError("image height and width must be divisible by patch_size")
        grid = (height // p, width // p)
        if grid[0] * grid[1] > self.config.max_patches:
            raise ValueError("number of patches exceeds max_patches")

        predicted_geometry = self.geometry_estimator(images)
        geometry_input = predicted_geometry if geometry is None else geometry
        if geometry_input.shape != predicted_geometry.shape:
            raise ValueError(
                "geometry must have shape "
                f"{tuple(predicted_geometry.shape)}, got {tuple(geometry_input.shape)}"
            )

        semantic = self.semantic(images)
        appearance = self.appearance(images)
        geometry_tokens = self.geometry(geometry_input)
        raw = torch.cat((semantic, appearance, geometry_tokens), dim=-1)
        fused = self.fusion(raw)
        coordinates = self._coordinates(
            images.shape[0], grid[0], grid[1], images.device, fused.dtype
        )

        fused, permutation = self.reorder(fused)
        token_index = permutation.unsqueeze(-1)
        raw = raw.gather(1, token_index.expand(-1, -1, raw.shape[-1]))
        coordinates = coordinates.gather(1, token_index.expand(-1, -1, 2))
        return VisualTokens(
            fused=fused,
            coordinates=coordinates,
            raw_channels=raw,
            predicted_geometry=predicted_geometry,
            permutation=permutation,
            grid_size=grid,
        )
