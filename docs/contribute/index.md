# Contributing

Perlang is at a very early state in its infancy, but do not let this scare you away from contributing anyway. Here are some suggestions:

- Proof-read this documentation web site. Find out obvious mistakes and submit fixes as GitHub pull requests. There is a convenient "Improve this Doc" link at the bottom of each page that simplifies the process of doing simple edits.

- Download the Perlang source code and try to build it on your local machine. Run the unit tests, to see if they work correctly with your operating system/.NET Core SDK version. If you have ideas for new unit tests, feel free to submit a PR. We are aiming for porting a number of tests from the [Lox test suite](https://github.com/munificent/craftinginterpreters/tree/master/test) where applicable, so there is plenty of room here for improvement. (Caveat: we don't support the full feature set from Lox yet, like being able to define instance methods in classes. But there are still many things we _do_ support but which ware lacking unit tests at the moment)

- File new issues about things that you think Perlang should support. This is **especially** of interest if you have worked with other programming languages before, that were either lacking some important feature, or _included_ a particular killer feature that you really think we should consider for inclusion in the Perlang code base as well. We make no promise to include your feature, but we _do_ promise to at least listen to what you have to say.

## The GitHub repository structure

- https://perlang.org - the web site you are currently watching.
- https://github.com/perlun/perlang/tree/master/docs - the source code to this documentation.
- https://github.com/perlun/perlang - the source code for the Perlang interpreter, stdlib etc.
