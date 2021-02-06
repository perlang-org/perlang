# Downloading Perlang

## Released versions

There are currently no "released" version of Perlang in that sense. We don't feel that the language & tooling is mature even to the point where it would make sense to call the version "0.1.0". Hopefully this will change in the future, but there is currently not timeframe available for when this will happen.

## Automated snapshot builds

Each commit to the `master` branch triggers a build that gets published as a set of `.tar.gz` files at https://builds.perlang.org (CDN sponsored by [Fastly](https://www.fastly.com/) - thanks you!). These builds are available for Linux, macOS and Windows.

The easiest way to install the latest build is by using the [perlang-install](https://github.com/perlang-org/perlang/tree/master/scripts/perlang-install) script. It works on all supported platforms (Linux, macOS and Windows - the latter requires a POSIX shell like Git Bash to be available). Use it like this:

[//]: # (Manually create Highlight.js fragments to ensure the space after the dollar sign is not selectable)

<pre><code class="lang-shell hljs"><span class="hljs-meta">$ </span><span class="bash">curl -sSL https://perlang.org/install.sh | sh</span>
</code></pre>

If you have a previous build installed and want to overwrite it, use the following:

<pre><code class="lang-shell hljs"><span class="hljs-meta">$ </span><span class="bash">curl -sSL https://perlang.org/install.sh | sh -s -- --force</span>
</code></pre>

**Note**: If you are running the installer in Git Bash on Windows, running `perlang` after installation will unfortunately not work in the Bash shell (because of limitations preventing us from reading individual keystrokes in that shell). Please close the `bash` session after installation and `cd %userprofile%\.perlang\nightly\bin`. You should then be able to run `perlang` to open up the Perlang console session.

For more details about this bug, please see https://github.com/perlang-org/perlang/issues/107.
