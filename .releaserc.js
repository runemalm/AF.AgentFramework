module.exports = {
    branches: ["master"],
    tagFormat: "v${version}",
    plugins: [
        [
            "@semantic-release/commit-analyzer",
            {
                preset: "conventionalcommits",
                presetConfig: { preMajor: true },
                releaseRules: [
                    { breaking: true, release: "minor" },
                    { type: "feat", release: "minor" },
                    { type: "fix", release: "patch" },
                    { type: "refactor", release: "patch" }
                ]
            }
        ],
        [
            "@semantic-release/release-notes-generator",
            {
                preset: "conventionalcommits",
                presetConfig: { preMajor: true },
                linkReferences: true,
                linkCompare: true,
                linkCommit: true,
                repositoryUrl: "https://github.com/runemalm/AF.AgentFramework",
                writerOpts: {
                    transform: (commit, context) => {
                        // Clone to avoid "Cannot modify immutable object"
                        const c = { ...commit };

                        // Keep all commit types (no filtering)
                        if (c.notes) {
                            c.notes.forEach(note => {
                                note.title = "BREAKING CHANGES";
                            });
                        }

                        if (c.subject) {
                            c.subject = c.subject.charAt(0).toUpperCase() + c.subject.slice(1);
                        }

                        if (c.hash) {
                            c.shortHash = c.hash.substring(0, 7);
                        }

                        return c;
                    }
                }
            }
        ],
        ["@semantic-release/changelog", { changelogFile: "CHANGELOG.md" }],
        [
            "@semantic-release/git",
            {
                assets: ["CHANGELOG.md"],
                message:
                    "chore(release): ${nextRelease.version} [skip ci]\n\n${nextRelease.notes}"
            }
        ],
        "@semantic-release/github"
    ]
};
