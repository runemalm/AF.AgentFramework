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

SRC_DIR := $(PWD)/src
TESTS_DIR := $(PWD)/tests
SAMPLES_DIR := $(PWD)/samples
ARTIFACTS_DIR := $(PWD)/artifacts

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
	dotnet test $(TESTS_DIR)/AgentFramework.Kernel.Tests/AgentFramework.Kernel.Tests.csproj

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
	dotnet pack $(SRC_DIR)/AgentFramework.Kernel/AgentFramework.Kernel.csproj -c Release -o $(ARTIFACTS_DIR)

##########################################################################
# RELEASE
##########################################################################

.PHONY: release-dry-run
release-dry-run: ##@Release	 Run semantic-release in dry-run mode (no tags/publish)
	@mkdir -p $(ARTIFACTS_DIR)
	@echo "Installing semantic-release (local dev deps)..."
	npm i -D semantic-release \
	      @semantic-release/commit-analyzer \
	      @semantic-release/release-notes-generator \
	      @semantic-release/changelog \
	      @semantic-release/git \
	      @semantic-release/github \
	      conventional-changelog-conventionalcommits
	@echo "Running semantic-release --dry-run ..."
	npx semantic-release --dry-run --no-ci 2>&1 | tee $(ARTIFACTS_DIR)/semantic-release-dry-run.log
	@echo ""
	@echo "${GREEN}Dry-run complete.${RESET} See $(ARTIFACTS_DIR)/semantic-release-dry-run.log"

##########################################################################
# SAMPLES
##########################################################################

.PHONY: hello-kernel
hello-kernel: ##@Samples	 Run the HelloKernel sample
	dotnet run --project samples/HelloKernel/HelloKernel.csproj
