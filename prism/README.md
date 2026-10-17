# Prism

Prism 是“原生空间感知多模态框架”的研究脚手架。它把视觉表示拆成等长的语义、外观和几何通道，通过 `what` 与 `where/how` 两条通路处理，并用条件流匹配头并行生成空间输出。

本目录包含：

- `docs/spec.md`：架构规格、张量契约、损失、训练阶段和证伪协议。
- `src/prism/`：只依赖 PyTorch 的最小参考实现。
- `configs/tiny.yaml`：可在 CPU 上做冒烟测试的配置示例。
- `examples/smoke_forward.py`：训练态前向和采样示例。
- `tests/`：形状、损失和采样的最小测试。

## 状态与边界

这是可执行的结构基线，不含预训练权重、数据集适配器或规模化训练基础设施。当前实现使用轻量卷积编码器、Transformer `what` 适配层和带相对坐标偏置的几何注意力，目的是固定接口并验证数据流，不代表论文中的最终规模或性能。

## 安装与运行

需要 Python 3.10+ 和 PyTorch 2.1+。

```powershell
cd prism
python -m pip install -e ".[dev]"
python examples/smoke_forward.py
python -m pytest
```

如果已有工程环境未安装 Python，可在任意具备 PyTorch 的环境中执行以上命令；本目录不修改父级 WinForms 解决方案。

## 最小 API

```python
import torch
from prism import PrismConfig, PrismModel

cfg = PrismConfig.tiny()
model = PrismModel(cfg)
images = torch.randn(2, 3, 64, 64)
targets = torch.randn(2, cfg.output_queries, cfg.output_dim)

out = model(images, spatial_targets=targets)
loss = out["losses"]["total"]
loss.backward()

samples = model.sample(images, steps=8)
```

设计详情见 [docs/spec.md](docs/spec.md)。
