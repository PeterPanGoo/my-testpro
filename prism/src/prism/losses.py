from __future__ import annotations

import torch
import torch.nn.functional as F
from torch import Tensor, nn

from .config import PrismConfig


def focal_regression_loss(
    prediction: Tensor,
    target: Tensor,
    gamma: float = 2.0,
    scale: float = 1.0,
) -> Tensor:
    """Smooth-L1 regression weighted toward currently hard elements."""

    absolute_error = (prediction - target).abs().detach()
    weight = (1.0 - torch.exp(-absolute_error / max(scale, 1e-6))).pow(gamma)
    return (weight * F.smooth_l1_loss(prediction, target, reduction="none")).mean()


class AuxiliaryPredictiveReconstruction(nn.Module):
    """Masks fused tokens and reconstructs the concatenated pre-fusion channels."""

    def __init__(self, config: PrismConfig) -> None:
        super().__init__()
        self.mask_ratio = config.apr_mask_ratio
        self.mask_token = nn.Parameter(torch.zeros(1, 1, config.model_dim))
        self.decoder = nn.Sequential(
            nn.LayerNorm(config.model_dim),
            nn.Linear(config.model_dim, config.model_dim * 2),
            nn.GELU(),
            nn.Linear(config.model_dim * 2, config.raw_token_dim),
        )

    def forward(self, fused: Tensor, target: Tensor) -> Tensor:
        mask = torch.rand(fused.shape[:2], device=fused.device) < self.mask_ratio
        # Guarantee a supervised position even for tiny batches/grids.
        if not mask.any():
            mask[0, 0] = True
        masked = torch.where(mask.unsqueeze(-1), self.mask_token, fused)
        prediction = self.decoder(masked)
        return F.smooth_l1_loss(prediction[mask], target[mask])


def flow_matching_batch(target: Tensor) -> tuple[Tensor, Tensor, Tensor, Tensor]:
    batch = target.shape[0]
    source = torch.randn_like(target)
    time = torch.rand(batch, device=target.device, dtype=target.dtype)
    time_view = time.view(batch, *([1] * (target.ndim - 1)))
    state = (1.0 - time_view) * source + time_view * target
    velocity = target - source
    return state, time, velocity, source
