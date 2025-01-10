# Contributing

Perlang is at a quite early state in its childhood, and directly contributing code changes is
perhaps challenging; it's unlikely to be obvious to outside contributors where the project is
heading. For now, this is probably where you can be of most help:

- Read existing issues, and give suggestions if you have thoughts on how particular things should be
  implemented/platforms that would be important to support/language features that you consider very
  useful, etc.

You can of course also download the Perlang tooling to your machine and attempt to run it, but
please be aware that since [the removal of the
REPL](https://github.com/perlang-org/perlang/pull/446), "toying around" with the Perlang interpreter
is much less obvious than what it used to be. Don't let this scare you away, though! It's just that
we are working on reimplementing Perlang as a fully AOT-compiled language (including the REPL), so
the intermediate result (version 0.5.0, 0.6.0 and current snapshots) might not be obviously useful
to outsiders. At the moment, Perlang is mostly useful for developing the Perlang compiler.

In the future, we hope to also provide a way to help fund the Perlang development financially. For
those of you interested in this, it will provide a very direct way to contribute to and benefit the
project. Stay tuned; we'll update this page whenever we are ready for this.

## GitLab repository structure & Perlang resources

- [perlang.org](https://perlang.org) - the web site you are currently watching.
- [gitlab.perlang.org/perlang/perlang](https://gitlab.perlang.org/perlang/perlang) - the source code for the Perlang interpreter, stdlib etc.
  - [repo.perlang.org](https://repo.perlang.org) - convenience shortlink to the GitLab repo
  - [issues.perlang.org](https://issues.perlang.org) - convenience shortlink to the list of open issues
- [builds.perlang.org](https://builds.perlang.org) - automated builds, updated on each commit to the `master` branch (powered by Fastly CDN)

There is also a [GitHub mirror](https://github.com/perlang-org/perlang) of our repository which is
instantly refreshed whenever new commits are being pushed to our GitLab instance. Feel free to clone
the project from whichever host you prefer, but Merge Requests/change suggestions should be directed
towards our GitLab instance for practical reasons.
