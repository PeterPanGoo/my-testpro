import pytest
import torch

from prism import PrismConfig, PrismModel


@pytest.fixture()
def setup():
    torch.manual_seed(0)
    config = PrismConfig.tiny()
    return config, PrismModel(config)


def test_training_forward_and_backward(setup):
    config, model = setup
    images = torch.randn(2, 3, 32, 32)
    geometry = torch.randn(2, config.geometry_channels, 32, 32)
    targets = torch.randn(2, config.output_queries, config.output_dim)

    result = model(images, geometry=geometry, spatial_targets=targets)

    assert result["context"].shape == (2, 16, config.model_dim)
    assert result["predicted_velocity"].shape == targets.shape
    assert set(result["losses"]) == {"apr", "geometry", "flow", "total"}
    assert torch.isfinite(result["losses"]["total"])
    result["losses"]["total"].backward()
    assert model.flow_head.velocity.weight.grad is not None


def test_sampling_is_parallel_and_shape_safe(setup):
    config, model = setup
    images = torch.randn(1, 3, 32, 32)
    samples = model.sample(images, steps=2)
    assert samples.shape == (1, config.output_queries, config.output_dim)
    assert torch.isfinite(samples).all()


def test_rejects_non_patch_aligned_images(setup):
    _, model = setup
    with pytest.raises(ValueError, match="divisible"):
        model(torch.randn(1, 3, 30, 32))
