## [0.12.0](https://github.com/runemalm/AF.AgentFramework/compare/v0.11.0...v0.12.0) (2025-10-12)

### feat

* **dashboard:** Show worker count ([66147f9](https://github.com/runemalm/AF.AgentFramework/commit/66147f9df800bf177891e4c01cbbd37957de8c9e))
* **kernel:** Add concurrent agent execution support ([f9c5c5f](https://github.com/runemalm/AF.AgentFramework/commit/f9c5c5f42fe7b501b22b66460cd42dc365e9a16d))

### chore

* **kernel:** Add worker count to snapshot ([bc8ff73](https://github.com/runemalm/AF.AgentFramework/commit/bc8ff73533672be7b302f3d9588d6a2d61febd9e))

## [0.11.0](https://github.com/runemalm/AF.AgentFramework/compare/v0.10.0...v0.11.0) (2025-10-12)

### feat

* **kernel:** Add scheduling policies for fair and balanced agent execution ([2edc6d1](https://github.com/runemalm/AF.AgentFramework/commit/2edc6d1c86b2db2b4e861214ab080494d4b8f207))

### chore

* **dashboard:** Add input field for custom refresh control ms ([3f6b6d7](https://github.com/runemalm/AF.AgentFramework/commit/3f6b6d7507ab3139e1554340286c8aecc5c6da32))
* **readme:** Add example code section ([d8756c7](https://github.com/runemalm/AF.AgentFramework/commit/d8756c7ca00b0881024386b210ca578b8f809add))

* Merge branch 'master' of github.com:runemalm/AF.AgentFramework ([cca8751](https://github.com/runemalm/AF.AgentFramework/commit/cca8751dce25a3394e1f72a3ee176bac21cf938b))

## [0.10.0](https://github.com/runemalm/AF.AgentFramework/compare/v0.9.0...v0.10.0) (2025-10-11)

### chore

* **agents:** Make HelloMapeAgent id configurable via constructor arg ([c95c6e7](https://github.com/runemalm/AF.AgentFramework/commit/c95c6e71430dda540d6f32016722f2b351fa208f))
* **ci:** Fix links in release notes ([85d4c18](https://github.com/runemalm/AF.AgentFramework/commit/85d4c18c781481c0a15e297a8e4a75fbe517e467))
* **readme:** Add dashboard screenshot to readme ([7e3d6ed](https://github.com/runemalm/AF.AgentFramework/commit/7e3d6ed1bc9946767040054fd346b7f3bb8f53f3))
* **readme:** Update nuget readme ([51670e9](https://github.com/runemalm/AF.AgentFramework/commit/51670e9d485f67196c6a17d5163b09e408754b3b))

### feat

* **observability:** Enhance dashboard with more metrics ([93617c0](https://github.com/runemalm/AF.AgentFramework/commit/93617c0676666f445c8822ebbc73eeb9591d4fd6))

## [0.9.0](https://github.com/runemalm/AF.AgentFramework/compare/v0.8.0...v0.9.0) (2025-10-11)

### chore

* **ci:** show all scopes in release notes ([](https://github.com/runemalm/AF.AgentFramework/commit/a463128a58ace35a534a71a347df5da0fcc12546))
* **hosting:** add AgentHostConsoleExtensions with RunConsoleAsync for graceful shutdown ([](https://github.com/runemalm/AF.AgentFramework/commit/1683eff54fb469d26e9e8cc841697f3a920ffb8b))
* **hosting:** extend AgentHostBuilder with AddHostService() and EnableDashboard() ([](https://github.com/runemalm/AF.AgentFramework/commit/0ddaabd1472441e24b6147abfa8bfec72f218fa8))
* **kernel:** implement IKernelInspector in InProcKernel ([](https://github.com/runemalm/AF.AgentFramework/commit/431da7996f1093efc23536e0c4ecfa75155b1a4c))
* **misc:** minor changes ([](https://github.com/runemalm/AF.AgentFramework/commit/fb578e60148abf7cf2397412e6dda7c1661b7723))
* **readme:** add minimal dashboard to roadmap ([](https://github.com/runemalm/AF.AgentFramework/commit/4e4bea8c019860c5e9153018662b3536415ab4a8))
* **samples:** update samples ([](https://github.com/runemalm/AF.AgentFramework/commit/ad9d687cd35cf3fafbba4311e89664c0de2e5cd6))
* update readme ([](https://github.com/runemalm/AF.AgentFramework/commit/061348b49ec7c3ea56a2dba452f19dd16d8ac868))

### feat

* **hosting:** add host service support (AgentHostService, ObservabilityDashboardService) ([](https://github.com/runemalm/AF.AgentFramework/commit/d795cf69e8d3629dadba3a9e9ee18d3e643e102d))
* **hosting:** add HTTP observability dashboard (ObservabilityServer) ([](https://github.com/runemalm/AF.AgentFramework/commit/03b1f5c1b0271db88c67fe02f61226b0494fd84b))
* **kernel:** add introspection API (IKernelInspector, KernelSnapshot) ([](https://github.com/runemalm/AF.AgentFramework/commit/44c67f18bef88c5ae873ceaf469ddacc42b82e49))

## [0.8.0](https://github.com/runemalm/AF.AgentFramework/compare/v0.7.0...v0.8.0) (2025-10-11)

### Features

* finish minimal version of reactive engine ([998b773](https://github.com/runemalm/AF.AgentFramework/commit/998b773c884f55ad89b6a6955a09a4cc1718b17d))

## [0.7.0](https://github.com/runemalm/AF.AgentFramework/compare/v0.6.0...v0.7.0) (2025-10-10)

### Features

* add AgentBase and MapekAgentBase ([a6fe247](https://github.com/runemalm/AF.AgentFramework/commit/a6fe2470f29c8a209cde6258364c6a32a7222a4b))
* add IKnowledge and InMemoryKnowledge ([e6c9151](https://github.com/runemalm/AF.AgentFramework/commit/e6c9151084e52a9d3b784805ced40dbf2456c228))

## [0.6.0](https://github.com/runemalm/AF.AgentFramework/compare/v0.5.1...v0.6.0) (2025-10-01)

### Features

* **tools:** add tools capability scaffolding ([3e2c2d5](https://github.com/runemalm/AF.AgentFramework/commit/3e2c2d5909e68621a9b9b096469258ecdd8ab80e))

## [0.5.1](https://github.com/runemalm/AF.AgentFramework/compare/v0.5.0...v0.5.1) (2025-09-21)

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
