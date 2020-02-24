#!/usr/bin/env ruby

SOLUTION_DIR = ARGV.pop or abort('Error: Solution directory must be provided')

GIT_VERSION = `git describe --always`.rstrip

common_constants = <<~EOF
//
// AUTO-GENERATED FILE, do not edit manually. Edit the scripts/update_common_constants.rb file instead.
//
namespace Perlang
{
    public static partial class CommonConstants
    {
        public const string GitVersion = "#{GIT_VERSION}";
    }
}
EOF

path = File.join(SOLUTION_DIR, 'Perlang.Common', 'CommonConstants.Generated.cs')
File.write(path, common_constants)

puts("#{path} updated")
