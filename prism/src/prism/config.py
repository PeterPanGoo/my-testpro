from dataclasses import dataclass


@dataclass(frozen=True)
class PrismConfig:
    image_channels: int = 3
    geometry_channels: int = 4
    patch_size: int = 8
    semantic_dim: int = 192
    appearance_dim: int = 128
    geometry_dim: int = 128
    model_dim: int = 384
    heads: int = 8
    what_layers: int = 6
    where_layers: int = 6
    cross_layers: int = 2
    ffn_multiplier: int = 4
    output_queries: int = 256
    output_dim: int = 4
    dropout: float = 0.1
    max_patches: int = 4096
    apr_mask_ratio: float = 0.25
    focal_gamma: float = 2.0
    focal_scale: float = 1.0
    flow_loss_weight: float = 1.0
    apr_loss_weight: float = 0.1
    geometry_loss_weight: float = 0.1

    def __post_init__(self) -> None:
        positive = {
            "image_channels": self.image_channels,
            "geometry_channels": self.geometry_channels,
            "patch_size": self.patch_size,
            "semantic_dim": self.semantic_dim,
            "appearance_dim": self.appearance_dim,
            "geometry_dim": self.geometry_dim,
            "model_dim": self.model_dim,
            "heads": self.heads,
            "output_queries": self.output_queries,
            "output_dim": self.output_dim,
            "max_patches": self.max_patches,
        }
        for name, value in positive.items():
            if value <= 0:
                raise ValueError(f"{name} must be positive, got {value}")
        if self.model_dim % self.heads:
            raise ValueError("model_dim must be divisible by heads")
        if not 0.0 <= self.apr_mask_ratio < 1.0:
            raise ValueError("apr_mask_ratio must be in [0, 1)")
        if not 0.0 <= self.dropout < 1.0:
            raise ValueError("dropout must be in [0, 1)")

    @property
    def raw_token_dim(self) -> int:
        return self.semantic_dim + self.appearance_dim + self.geometry_dim

    @classmethod
    def tiny(cls) -> "PrismConfig":
        return cls(
            semantic_dim=32,
            appearance_dim=24,
            geometry_dim=24,
            model_dim=64,
            heads=4,
            what_layers=2,
            where_layers=2,
            cross_layers=1,
            ffn_multiplier=2,
            output_queries=16,
            dropout=0.0,
            max_patches=256,
        )
