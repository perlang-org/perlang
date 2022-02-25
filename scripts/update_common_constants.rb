#!/usr/bin/env ruby

# Need to take the dev/ part out of e.g. dev/0.1.0, since assembly versions must
# adhere to the following format: major[.minor[.build[.revision]]]
GIT_TAG_VERSION = `git describe --tags --abbrev=0 | sed s%^dev/%%`.rstrip

# The input to these sed operations is something like `dev/0.1.0-224-g09f4704`.
# The output is expected to produce a SemVer-compliant version number. For
# snapshots, it will be something like 0.1.0-dev.224
GIT_DESCRIBE_VERSION = `git describe --tags | sed s%^dev/%% | sed s/-g.*$// | sed -E "s/-([0-9]*)$/-dev.\\1/"`.rstrip

GIT_COMMIT = `git describe --always`.rstrip

common_constants = <<~EOF
//
// AUTO-GENERATED FILE, do not edit manually. Edit the scripts/update_common_constants.rb file instead.
//
namespace Perlang
{
    public static partial class CommonConstants
    {
        public const string GitTagVersion = "#{GIT_TAG_VERSION}";
        public const string GitDescribeVersion = "#{GIT_DESCRIBE_VERSION}";
        public const string GitCommit = "#{GIT_COMMIT}";
    }
}
EOF

path = File.join('src', 'Perlang.Common', 'CommonConstants.Generated.cs')
File.write(path, common_constants)

puts("#{path} updated")
