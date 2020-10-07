# Downloading Perlang

## Released versions

There are currently no "released" version of Perlang in that sense. We don't feel that the language & tooling is mature even to the point where it would make sense to call the version "0.1.0". Hopefully this will change in the future, but there is currently not timeframe available for when this will happen.

## Automated snapshot builds

Each commit to the `master` branch triggers a build that gets published as a set of `.tar.gz` files at https://builds.perlang.org (CDN sponsored by [Fastly](https://www.fastly.com/) - thanks you!). These builds are available for Linux, macOS and Windows.

The easiest way to install the latest build is by using the [perlang-install](https://github.com/perlun/perlang/tree/master/scripts/perlang-install) script. It works on all supported platforms (Linux, macOS and Windows - the latter requires a POSIX shell like Git Bash to be available). Use it like this:

```shell
$ curl -sSL https://perlang.org/install | sh
```

If you have a previous build installed and want to overwrite it, use the following:

```shell
$ curl -sSL https://perlang.org/install | sh -s -- --force