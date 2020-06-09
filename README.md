![Build](https://github.com/perlun/perlang/workflows/.NET%20Core/badge.svg)

# perlang

The Perlang Programming Language

## Builds

Each commit to the `master` branch triggers a build that gets published as a set of `.tar.gz` files at [Bintray](https://bintray.com/perlang/builds/perlang/build#files). Binaries are available for Linux, macOS and Windows.

## Installing prerequisites for building

```shell
$ sudo apt-get install \
      dotnet-sdk-3.1 \
      make \
      ruby
```

## How to build the Perlang interpreter

```shell
$ git clone https://github.com/perlun/perlang.git
$ cd perlang
$ make
```

You should now have a `perlang` executable. Run `make run` to run it. If you are brave, `make install` (currently Linux only) will put binaries in a path under `~/.perlang/bin` which you can put in your `$PATH` so you can easily use it from anywhere. [This commit](https://github.com/perlun/dotfiles/commit/c168c50afac0f8e7e099e3843e1228d8a3ae75d0) from my dotfiles repo can be used as inspiration for adding this folder to your `$PATH`.

## Documentation

- [docs/syntax-grammar.md](docs/syntax-grammar.md): Specification of the syntax grammar for the Perlang language.

## License

[MIT](LICENSE)

[perlang-install](scripts/perlang-install) is originally based on [rustup-init](https://github.com/rust-lang/rustup/blob/master/rustup-init.sh), which is also licensed under the MIT license.
