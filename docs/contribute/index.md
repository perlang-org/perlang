# Contributing

Perlang is at a very early state in its infancy, but do not let this scare you away from contributing anyway. Here are some suggestions:

- Proof-read this documentation web site. Find out obvious mistakes and submit fixes as GitLab merge requests. There is a convenient "Improve this Doc" link at the right-hand side of each page that simplifies the process of doing simple edits. (**Update*: This doesn't work since [the move to GitLab](https://gitlab.perlang.org/perlang/perlang/-/issues/423)) For more extensive changes, making a proper fork & pulling down the repo locally is probably more practical.

- Download the Perlang source code and try to build it on your local machine. Run the unit tests, to see if they work correctly with your operating system/.NET SDK version. If you have ideas for new unit tests, feel free to submit an MR. Smaller things are fine to just hack up and then submit an MR about. For larger things, to avoid wasting both your and our time, make sure to file an issue (see below) first and get an indication from the team that this is indeed a test that we should have.

- [File new issues](https://gitlab.perlang.org/perlang/perlang/-/issues) about things that you think Perlang should support. This is **especially** of interest if you have worked with other programming languages before, that were either lacking some important feature, or _included_ a particular killer feature that you really think we should consider for inclusion in the Perlang code base as well. We make no promise to include your feature, but we _do_ promise to at least listen to what you have to say.

## GitLab repository structure & Perlang resources

- https://perlang.org - the web site you are currently watching.
- https://gitlab.perlang.org/perlang/perlang/-/tree/master/docs - the source code to this documentation.
- https://gitlab.perlang.org/perlang/perlang - the source code for the Perlang interpreter, stdlib etc.
  - https://repo.perlang.org - convenience shortlink to the GitLab repo
- https://builds.perlang.org - automated builds, updated on each commit to the `master` branch
