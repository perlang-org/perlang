# Agent Guidelines

Follow these repo-specific rules when editing code:

## Commits
- Commit subject: concise, ideally 50 chars, max 72.
- Commit body: wrap at 72 chars; include why the change was made.
- Add `Co-Authored-By` when an agent contributes materially.
- Release notes: add an entry for every change. Ask the user for the MR number.
- C# code: one type per file. Do not put multiple classes/interfaces in the same file.
- Error messages and user-facing strings must be in English, even if the user writes in another language.

### Commit Title Prefix
- Prefer a scope prefix in parentheses, e.g. `(compiler)`, `(language)`, `(parser)`, `(stdlib)`, `(tests)`, `(ci)`,
  `(release-notes)`, `(docs)`, `(README)`, `(Makefile)`, `(perlang_cli)`, `(common)`, `(many)`.
- Examples from recent history: `(compiler) Improve error handling when running \`clang\``,
  `(language) Support \`static\` methods in classes`, `(ci) Ensure CppSharp bindings are always up-to-date`,
  `(release-notes) Add entry for latest commit`.

## Tests and CI
- If unsure, ask which tests to run. Prefer targeted tests over full suite.
- Default integration test command:
  `dotnet test src/Perlang.Tests.Integration/Perlang.Tests.Integration.csproj`
