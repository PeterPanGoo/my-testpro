from __future__ import annotations

import math

import torch
import torch.nn.functional as F
from torch import Tensor, nn

from .config import PrismConfig


class RelativeGeometryAttention(nn.Module):
    """Self-attention with learned 2-D displacement bias per attention head."""

    def __init__(self, dim: int, heads: int, dropout: float) -> None:
        super().__init__()
        self.heads = heads
        self.head_dim = dim // heads
        self.qkv = nn.Linear(dim, dim * 3)
        self.relative_bias = nn.Sequential(
            nn.Linear(3, max(16, heads * 2)), nn.GELU(), nn.Linear(max(16, heads * 2), heads)
        )
        self.output = nn.Linear(dim, dim)
        self.dropout = dropout

    def forward(self, tokens: Tensor, coordinates: Tensor) -> Tensor:
        batch, count, dim = tokens.shape
        qkv = self.qkv(tokens).reshape(batch, count, 3, self.heads, self.head_dim)
        query, key, value = qkv.unbind(dim=2)
        query = query.transpose(1, 2)
        key = key.transpose(1, 2)
        value = value.transpose(1, 2)

        delta = coordinates[:, :, None, :] - coordinates[:, None, :, :]
        distance = delta.square().sum(dim=-1, keepdim=True).sqrt()
        geometry_features = torch.cat((delta, distance), dim=-1)
        bias = self.relative_bias(geometry_features).permute(0, 3, 1, 2)
        scores = torch.matmul(query, key.transpose(-2, -1)) / math.sqrt(self.head_dim)
        weights = F.softmax(scores + bias, dim=-1)
        weights = F.dropout(weights, p=self.dropout, training=self.training)
        attended = torch.matmul(weights, value).transpose(1, 2).reshape(batch, count, dim)
        return self.output(attended)


class GeometryBlock(nn.Module):
    def __init__(self, config: PrismConfig) -> None:
        super().__init__()
        dim = config.model_dim
        hidden = dim * config.ffn_multiplier
        self.norm1 = nn.LayerNorm(dim)
        self.attention = RelativeGeometryAttention(dim, config.heads, config.dropout)
        self.norm2 = nn.LayerNorm(dim)
        self.ffn = nn.Sequential(
            nn.Linear(dim, hidden),
            nn.GELU(),
            nn.Dropout(config.dropout),
            nn.Linear(hidden, dim),
        )
        self.dropout = nn.Dropout(config.dropout)

    def forward(self, tokens: Tensor, coordinates: Tensor) -> Tensor:
        tokens = tokens + self.dropout(self.attention(self.norm1(tokens), coordinates))
        return tokens + self.dropout(self.ffn(self.norm2(tokens)))


class WhatPathway(nn.Module):
    """Replaceable semantic backbone adapter; a Transformer is used by default."""

    def __init__(self, config: PrismConfig) -> None:
        super().__init__()
        layer = nn.TransformerEncoderLayer(
            d_model=config.model_dim,
            nhead=config.heads,
            dim_feedforward=config.model_dim * config.ffn_multiplier,
            dropout=config.dropout,
            activation="gelu",
            batch_first=True,
            norm_first=True,
        )
        self.encoder = nn.TransformerEncoder(
            layer, num_layers=config.what_layers, norm=nn.LayerNorm(config.model_dim)
        )

    def forward(self, tokens: Tensor) -> Tensor:
        return self.encoder(tokens)


class WherePathway(nn.Module):
    def __init__(self, config: PrismConfig) -> None:
        super().__init__()
        self.layers = nn.ModuleList(
            GeometryBlock(config) for _ in range(config.where_layers)
        )
        self.norm = nn.LayerNorm(config.model_dim)

    def forward(self, tokens: Tensor, coordinates: Tensor) -> Tensor:
        for layer in self.layers:
            tokens = layer(tokens, coordinates)
        return self.norm(tokens)


class BidirectionalCrossPath(nn.Module):
    def __init__(self, config: PrismConfig) -> None:
        super().__init__()
        self.what_norm = nn.LayerNorm(config.model_dim)
        self.where_norm = nn.LayerNorm(config.model_dim)
        self.what_reads_where = nn.MultiheadAttention(
            config.model_dim, config.heads, config.dropout, batch_first=True
        )
        self.where_reads_what = nn.MultiheadAttention(
            config.model_dim, config.heads, config.dropout, batch_first=True
        )

    def forward(self, what: Tensor, where: Tensor) -> tuple[Tensor, Tensor]:
        what_query = self.what_norm(what)
        where_query = self.where_norm(where)
        what_delta, _ = self.what_reads_where(what_query, where_query, where_query)
        where_delta, _ = self.where_reads_what(where_query, what_query, what_query)
        return what + what_delta, where + where_delta
