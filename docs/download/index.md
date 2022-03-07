# Downloading Perlang

## Released versions

From time to time, we publish a release version of Perlang at https://github.com/perlang-org/perlang/releases. These releases are useful if you want to have something a little more "stable" than the automated snapshots, but please be aware that these releases are still not recommended for "production usage" unless you are very brave.

For most of you, the automated snapshots are more suitable than the release versions, since they include all the latest bug fixes and new features.

<pre><code class="lang-shell hljs"><span class="hljs-meta">$ </span><span class="bash">curl -sSL https://perlang.org/install.sh | sh -s -- --default-toolchain release</span>
</code></pre>

If you have a previous release installed and want to overwrite it, use the following command line:

<pre><code class="lang-shell hljs"><span class="hljs-meta">$ </span><span class="bash">curl -sSL https://perlang.org/install.sh | sh -s -- --default-toolchain release --force</span>
</code></pre>

## Automated snapshot builds

In addition to the above, each commit to the `master` branch triggers a build that gets published as a set of `.tar.gz` files at https://builds.perlang.org (CDN sponsored by [Fastly](https://www.fastly.com/) - thank you!). These builds are available for Linux, macOS and Windows.

The easiest way to install the latest build is by using the [perlang-install](https://github.com/perlang-org/perlang/tree/master/scripts/perlang-install) script. It works on all supported platforms (Linux (`arm`, `arm64` and `x64`), macOS (`x64`) and Windows (`x64`) - the latter requires a POSIX shell like Git Bash to be available). Use it like this:

<pre><code class="lang-shell hljs"><span class="hljs-meta">$ </span><span class="bash">curl -sSL https://perlang.org/install.sh | sh</span>
</code></pre>

Just as with the stable releses, if you have a previous build installed and want to overwrite it, you need to enable this in the installer. Use the following command line:

<pre><code class="lang-shell hljs"><span class="hljs-meta">$ </span><span class="bash">curl -sSL https://perlang.org/install.sh | sh -s -- --force</span>
</code></pre>

## Video tutorial

For those of you who prefer to learn by watching videos, here's a short screencast (courtesy of [Asciinema](https://asciinema.org/)) which shows what the installer looks like when you run it:

<asciinema-player cols="177" rows="28" speed="2" src="/casts/perlang-install.cast"></asciinema-player>
