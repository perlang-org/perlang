#!/usr/bin/env ruby

SOLUTION_DIR = ARGV.pop or abort('Error: Solution directory must be provided')

# Need to take -dev part out of e.g. 1.0.0-dev, since assembly versions must
# adhere to the following format: major[.minor[.build[.revision]]]
GIT_TAG_VERSION = `git describe --tags --abbrev=0 | sed s/-dev//`.rstrip

GIT_DESCRIBE_VERSION = `git describe --tags | sed s/-g.*$// | sed s/dev-/dev./`.rstrip
GIT_REVISION = `git describe --always`.rstrip

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
        public const string GitRevision = "#{GIT_REVISION}";
    }
}
EOF

path = File.join(SOLUTION_DIR, 'Perlang.Common', 'CommonConstants.Generated.cs')
File.write(path, common_constants)

puts("#{path} updated")
