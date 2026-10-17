from __future__ import annotations

from typing import Any

import torch
from torch import Tensor, nn

from .config import PrismConfig
from .encoder import UnifiedVisualEncoder, VisualTokens
from .flow import FlowMatchingHead
from .losses import (
    AuxiliaryPredictiveReconstruction,
    flow_matching_batch,
    focal_regression_loss,
)
from .pathways import BidirectionalCrossPath, WhatPathway, WherePathway


class PrismModel(nn.Module):
    def __init__(self, config: PrismConfig) -> None:
        super().__init__()
        self.config = config
        self.visual_encoder = UnifiedVisualEncoder(config)
        self.what_pathway = WhatPathway(config)
        self.where_pathway = WherePathway(config)
        self.cross_path = nn.ModuleList(
            BidirectionalCrossPath(config) for _ in range(config.cross_layers)
        )
        self.context_fusion = nn.Sequential(
            nn.LayerNorm(config.model_dim * 2),
            nn.Linear(config.model_dim * 2, config.model_dim),
        )
        self.flow_head = FlowMatchingHead(config)
        self.apr = AuxiliaryPredictiveReconstruction(config)

    def encode(self, images: Tensor, geometry: Tensor | None = None) -> dict[str, Any]:
        visual = self.visual_encoder(images, geometry)
        what = self.what_pathway(visual.fused)
        where = self.where_pathway(visual.fused, visual.coordinates)
        for exchange in self.cross_path:
            what, where = exchange(what, where)
        context = self.context_fusion(torch.cat((what, where), dim=-1))
        return {"visual": visual, "what": what, "where": where, "context": context}

    def forward(
        self,
        images: Tensor,
        geometry: Tensor | None = None,
        spatial_targets: Tensor | None = None,
    ) -> dict[str, Any]:
        encoded = self.encode(images, geometry)
        visual: VisualTokens = encoded["visual"]
        result: dict[str, Any] = {**encoded, "losses": {}}

        apr_loss = self.apr(visual.fused, visual.raw_channels)
        losses = result["losses"]
        losses["apr"] = apr_loss
        total = self.config.apr_loss_weight * apr_loss

        if geometry is not None:
            geometry_loss = focal_regression_loss(
                visual.predicted_geometry,
                geometry,
                gamma=self.config.focal_gamma,
                scale=self.config.focal_scale,
            )
            losses["geometry"] = geometry_loss
            total = total + self.config.geometry_loss_weight * geometry_loss

        if spatial_targets is not None:
            expected = (
                images.shape[0],
                self.config.output_queries,
                self.config.output_dim,
            )
            if spatial_targets.shape != expected:
                raise ValueError(
                    f"spatial_targets must have shape {expected}, "
                    f"got {tuple(spatial_targets.shape)}"
                )
            state, time, target_velocity, _ = flow_matching_batch(spatial_targets)
            predicted_velocity = self.flow_head(state, time, encoded["context"])
            flow_loss = focal_regression_loss(
                predicted_velocity,
                target_velocity,
                gamma=self.config.focal_gamma,
                scale=self.config.focal_scale,
            )
            result["predicted_velocity"] = predicted_velocity
            losses["flow"] = flow_loss
            total = total + self.config.flow_loss_weight * flow_loss

        losses["total"] = total
        return result

    @torch.no_grad()
    def sample(
        self,
        images: Tensor,
        geometry: Tensor | None = None,
        steps: int = 32,
        noise: Tensor | None = None,
    ) -> Tensor:
        encoded = self.encode(images, geometry)
        return self.flow_head.sample(encoded["context"], steps=steps, noise=noise)
