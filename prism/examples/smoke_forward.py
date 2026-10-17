import torch

from prism import PrismConfig, PrismModel


def main() -> None:
    torch.manual_seed(7)
    config = PrismConfig.tiny()
    model = PrismModel(config)
    images = torch.randn(2, 3, 64, 64)
    geometry = torch.randn(2, config.geometry_channels, 64, 64)
    targets = torch.randn(2, config.output_queries, config.output_dim)

    output = model(images, geometry=geometry, spatial_targets=targets)
    output["losses"]["total"].backward()
    samples = model.sample(images, geometry=geometry, steps=4)

    print("context:", tuple(output["context"].shape))
    print("samples:", tuple(samples.shape))
    print("losses:", {key: float(value.detach()) for key, value in output["losses"].items()})


if __name__ == "__main__":
    main()
