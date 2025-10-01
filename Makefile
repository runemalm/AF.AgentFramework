##########################################################################
# This is the project's makefile.
#
# Simply run 'make' on the command line to list all available tasks.
##########################################################################

##########################################################################
# CONFIGURATION
##########################################################################

# Load env file
include env.make
export $(shell sed 's/=.*//' env.make)

##########################################################################
# VARIABLES
##########################################################################

HOME := $(shell echo ~)
PWD := $(shell pwd)

SRC_DIR 	  := $(PWD)/src
TESTS_DIR 	  := $(PWD)/tests
SAMPLES_DIR   := $(PWD)/samples
ARTIFACTS_DIR := $(PWD)/artifacts

DOCS_DIR           := $(PWD)/docs
DOCS_OUT_DIR       := $(ARTIFACTS_DIR)/docs
DOCS_SITE_DIR      := $(DOCS_OUT_DIR)/site

TFM                := net8.0
PROJECT_NAME 	   := AF.AgentFramework
PROJECT_CSPROJ     := $(SRC_DIR)/$(PROJECT_NAME)/$(PROJECT_NAME).csproj

TOOLS_MANIFEST 	   := $(PWD)/.config/dotnet-tools.json

##########################################################################
# HELP
##########################################################################

BLUE      := $(shell tput -Txterm setaf 4)
GREEN     := $(shell tput -Txterm setaf 2)
TURQUOISE := $(shell tput -Txterm setaf 6)
WHITE     := $(shell tput -Txterm setaf 7)
YELLOW    := $(shell tput -Txterm setaf 3)
GREY      := $(shell tput -Txterm setaf 1)
RESET     := $(shell tput -Txterm sgr0)
SMUL      := $(shell tput smul)
RMUL      := $(shell tput rmul)

# Add the following 'help' target to your Makefile
# And add help text after each target name starting with '\#\#'
# A category can be added with @category
HELP_FUN = \
	%help; \
	use Data::Dumper; \
	while(<>) { \
		if (/^([a-zA-Z\-_0-9]+)\s*:.*\#\#(?:@([a-zA-Z\-0-9\.\s]+))?\t(.*)$$/) { \
			$$c = $$2; $$t = $$1; $$d = $$3; \
			push @{$$help{$$c}}, [$$t, $$d, $$ARGV] unless grep { grep { grep /^$$t$$/, $$_->[0] } @{$$help{$$_}} } keys %help; \
		} \
	}; \
	for (sort keys %help) { \
		printf("${WHITE}%24s:${RESET}\n\n", $$_); \
		for (@{$$help{$$_}}) { \
			printf("%s%25s${RESET}%s  %s${RESET}\n", \
				( $$_->[2] eq "Makefile" || $$_->[0] eq "help" ? "${YELLOW}" : "${GREY}"), \
				$$_->[0], \
				( $$_->[2] eq "Makefile" || $$_->[0] eq "help" ? "${GREEN}" : "${GREY}"), \
				$$_->[1] \
			); \
		} \
		print "\n"; \
	}

# make
.DEFAULT_GOAL := help

# Variable wrapper
define defw
	custom_vars += $(1)
	$(1) ?= $(2)
	export $(1)
	shell_env += $(1)="$$($(1))"
endef

.PHONY: help
help:: ##@Other Show this help.
	@echo ""
	@printf "%30s " "${YELLOW}TARGETS"
	@echo "${RESET}"
	@echo ""
	@perl -e '$(HELP_FUN)' $(MAKEFILE_LIST)

##########################################################################
# TEST
##########################################################################

.PHONY: test
test: ##@Test	 Run all tests
	dotnet test $(TESTS_DIR)/$(PROJECT_NAME).Tests/$(PROJECT_NAME).Tests.csproj

##########################################################################
# BUILD
##########################################################################

.PHONY: build
build: ##@Build	 build the solution
	dotnet build

.PHONY: deep-rebuild
deep-rebuild: ##@Build	 clean, clear nuget caches, restore and build the project
	make clean
	make clear-nuget-caches
	make restore
	make build

.PHONY: clean
clean: ##@Build	 clean the solution
	find . $(SRC_DIR) -iname "bin" | xargs rm -rf
	find . $(SRC_DIR) -iname "obj" | xargs rm -rf

.PHONY: restore
restore: ##@Build	 restore the solution
	dotnet restore

.PHONY: clear-nuget-caches
clear-nuget-caches: ##@Build	 clean all nuget caches
	dotnet nuget locals all --clear

.PHONY: pack
pack: ##@Build	 pack the nuget
	dotnet pack $(PROJECT_CSPROJ) -c Release -o $(ARTIFACTS_DIR)

##########################################################################
# RELEASE
##########################################################################

.PHONY: release-dry-run
release-dry-run: ##@Release	 Run semantic-release in dry-run mode (no tags/publish)
	npx semantic-release --dry-run --no-ci

##########################################################################
# SAMPLES
##########################################################################

.PHONY: hello-kernel
hello-kernel: ##@Samples	 Run the HelloKernel sample
	dotnet run --project samples/HelloKernel/HelloKernel.csproj

##########################################################################
# DOCS
##########################################################################

.PHONY: docs-site-tools
docs-site-tools: ##@Docs	Install DocFX as a local dotnet tool
	@test -f $(TOOLS_MANIFEST) || dotnet new tool-manifest
	dotnet tool install docfx --version 2.78.3 || dotnet tool update docfx

.PHONY: docs-site-init
docs-site-init: docs-site-tools ##@Docs	Initialize a DocFX project in ./docs (one-time)
	@mkdir -p "$(DOCS_DIR)"
	@if [ ! -f "$(DOCS_DIR)/docfx.json" ]; then \
	  cd "$(DOCS_DIR)" && dotnet tool run docfx init --yes; \
	  echo "DocFX initialized under: $(DOCS_DIR)"; \
	else \
	  echo "DocFX already initialized at: $(DOCS_DIR)"; \
	fi

.PHONY: docs-site
docs-site: docs-site-tools ##@Docs	Build the DocFX static site into artifacts/docs/site
	@mkdir -p "$(DOCS_SITE_DIR)"
	@if [ ! -f "$(DOCS_DIR)/docfx.json" ]; then \
	  echo "docfx.json not found. Run 'make docs-site-init' first."; \
	  exit 1; \
	fi
	# Ensure the XML docs exist
	dotnet build $(PROJECT_CSPROJ) -c Release
	# Generate API metadata (YAML) from the csproj
	cd "$(DOCS_DIR)" && dotnet tool run docfx metadata
	# Build the static site
	cd "$(DOCS_DIR)" && dotnet tool run docfx build -o "$(DOCS_SITE_DIR)"
	@echo "DocFX site built at: $(DOCS_SITE_DIR)"
	
.PHONY: docs-serve
docs-serve: ##@Docs	Serve DocFX site locally (http://localhost:8080)
	@cd "$(DOCS_SITE_DIR)" && python3 -m http.server 8080
