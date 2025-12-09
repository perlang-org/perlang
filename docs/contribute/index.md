# Contributing

Perlang has been developed for a few years now, but is still at a quite an early state in its
childhood, and directly contributing code changes can be challenging; it's unlikely to be obvious to
outside contributors where the project is heading. However, you can still participate in the project
in e.g. the following ways:

- Read the commit logs as new changes are being published. We try to write meaningful git commit
  messages, where the first line gives the overall picture and the "body" of the commit message
  might provide more details. Each commit pushed will produce a snapshot version of the tooling that
  you can install using our [download page](../download).

- Poke around in existing [GitLab issues](https://issues.perlang.org). While project management is
  perhaps not our favorite chore, we try to keep these up-to-date. If you want to see where the
  project is heading, this will give you some ideas.

- Read [our mailing lists](https://lists.perlang.org). We will post project updates to these mailing
  lists from time to time. If you want to get in touch with the people behind the project, this is
  your best bet. You are welcome to subscribe to the mailing lists if you like; once you have
  requested to be subscribed, we will manually confirm you before you get subscribed to the lits.
  Here is some basic netiquette that we would appreciate if you adhere to:
    * Use common sense.
    * Stay on topic.
    * If possible with your mail client, [use plain text email](https://useplaintext.email/).

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
