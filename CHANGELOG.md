## [1.0.0](https://github.com/runemalm/AF.AgentFramework/compare/v0.5.0...v1.0.0) (2025-09-21)

### ⚠ BREAKING CHANGES

* **kernel,hosting,engines,runners,policies,docs,ci:** repository and package have been restructured.
- Namespaces unified under `AgentFramework.*`; assembly/package is `AF.AgentFramework`
- Previous `AgentFramework.Kernel` code has been replaced
- Package ID changed to `AF.AgentFramework` and public API is not backward compatible

### Features

* **kernel,hosting,engines,runners,policies,docs,ci:** introduce AF.AgentFramework runtime, docs site, and release pipeline ([0760321](https://github.com/runemalm/AF.AgentFramework/commit/0760321c54351a4a090773b168f33940ddc18365))

## [0.5.0](https://github.com/runemalm/AgentFramework.Kernel/compare/v0.4.1...v0.5.0) (2025-09-13)

### Features

* refactor to more generic kernel using step-based loop profiles and middleware pipeline ([6b180a9](https://github.com/runemalm/AgentFramework.Kernel/commit/6b180a919bebdc4921904bf1b44fb4cddf0f6840))

## [0.5.0](https://github.com/runemalm/AgentFramework.Kernel/compare/v0.4.1...v0.5.0) (2025-09-13)

### Features

* refactor to more generic kernel using step-based loop profiles and middleware pipeline ([6b180a9](https://github.com/runemalm/AgentFramework.Kernel/commit/6b180a919bebdc4921904bf1b44fb4cddf0f6840))

## [0.4.1](https://github.com/runemalm/AgentFramework.Kernel/compare/v0.4.0...v0.4.1) (2025-08-31)


### Bug Fixes

* **kernel:** propagate exceptions/cancellation from StepAsync ([e1400fd](https://github.com/runemalm/AgentFramework.Kernel/commit/e1400fd83dd51b5afbd58a076340f1d7fc1e8c69))

# [0.4.0](https://github.com/runemalm/AgentFramework.Kernel/compare/v0.3.0...v0.4.0) (2025-08-30)


### Features

* **kernel:** add core policy decorators and router ([c8f23c5](https://github.com/runemalm/AgentFramework.Kernel/commit/c8f23c524e1b507c403afb3abef3ebe2c8abfe12))

# [0.3.0](https://github.com/runemalm/AgentFramework.Kernel/compare/v0.2.0...v0.3.0) (2025-08-29)


### Features

* **extras:** add initial OpenAI adapter and Files tool projects with test stubs ([d48faf5](https://github.com/runemalm/AgentFramework.Kernel/commit/d48faf547d6791e0c4e75beabcae825e3c20bff1))

# [0.2.0](https://github.com/runemalm/AgentFramework.Kernel/compare/v0.1.0...v0.2.0) (2025-08-28)


### Features

* add first version ([4d86b10](https://github.com/runemalm/AgentFramework.Kernel/commit/4d86b101bdc4852c30998c2fd75640339d37901b))
