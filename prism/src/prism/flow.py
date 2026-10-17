from __future__ import annotations

import math

import torch
from torch import Tensor, nn

from .config import PrismConfig


class TimeEmbedding(nn.Module):
    def __init__(self, dim: int) -> None:
        super().__init__()
        self.dim = dim
        self.projection = nn.Sequential(nn.Linear(dim, dim), nn.SiLU(), nn.Linear(dim, dim))

    def forward(self, time: Tensor) -> Tensor:
        half = self.dim // 2
        frequencies = torch.exp(
            torch.linspace(0.0, -math.log(10_000.0), half, device=time.device, dtype=time.dtype)
        )
        angles = time[:, None] * frequencies[None, :]
        embedding = torch.cat((angles.sin(), angles.cos()), dim=-1)
        if embedding.shape[-1] < self.dim:
            embedding = torch.cat((embedding, embedding.new_zeros(time.shape[0], 1)), dim=-1)
        return self.projection(embedding)


class FlowMatchingHead(nn.Module):
    """Conditional velocity field over a fixed set of parallel spatial slots."""

    def __init__(self, config: PrismConfig) -> None:
        super().__init__()
        dim = config.model_dim
        self.output_queries = config.output_queries
        self.output_dim = config.output_dim
        self.slot_embedding = nn.Parameter(torch.randn(1, config.output_queries, dim) * 0.02)
        self.state_projection = nn.Linear(config.output_dim, dim)
        self.time_embedding = TimeEmbedding(dim)
        self.cross_attention = nn.MultiheadAttention(
            dim, config.heads, config.dropout, batch_first=True
        )
        layer = nn.TransformerEncoderLayer(
            dim,
            config.heads,
            dim * config.ffn_multiplier,
            config.dropout,
            activation="gelu",
            batch_first=True,
            norm_first=True,
        )
        self.decoder = nn.TransformerEncoder(layer, num_layers=2, norm=nn.LayerNorm(dim))
        self.velocity = nn.Linear(dim, config.output_dim)

    def forward(self, state: Tensor, time: Tensor, context: Tensor) -> Tensor:
        expected = (context.shape[0], self.output_queries, self.output_dim)
        if state.shape != expected:
            raise ValueError(f"state must have shape {expected}, got {tuple(state.shape)}")
        query = self.slot_embedding.expand(state.shape[0], -1, -1)
        query = query + self.state_projection(state) + self.time_embedding(time)[:, None, :]
        conditioned, _ = self.cross_attention(query, context, context)
        return self.velocity(self.decoder(query + conditioned))

    @torch.no_grad()
    def sample(self, context: Tensor, steps: int = 32, noise: Tensor | None = None) -> Tensor:
        if steps <= 0:
            raise ValueError("steps must be positive")
        shape = (context.shape[0], self.output_queries, self.output_dim)
        state = torch.randn(shape, device=context.device, dtype=context.dtype) if noise is None else noise
        if state.shape != shape:
            raise ValueError(f"noise must have shape {shape}, got {tuple(state.shape)}")
        step_size = 1.0 / steps
        for index in range(steps):
            time = state.new_full((state.shape[0],), index / steps)
            state = state + step_size * self(state, time, context)
        return state
