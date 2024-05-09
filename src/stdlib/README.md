# stdlib

This is the Perlang standard library. It is currently written in C++. As the Perlang compiler and ecosystem matures, we
expect some of this to be rewritten in Perlang but there will likely always be a need for some C++ code.

## Style

We follow the [Google C++ style guide](https://google.github.io/styleguide/cppguide.html) loosely. This means:

* Use `.cc` and `.h` for file names.
* Use `snake_case` for variable names.
* Use `CamelCase` for class names.
* Use `field_` for member variables, i.e. trailing underscore.
