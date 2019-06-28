#!/usr/bin/env ruby

# Author: Lev Trubov
# © 2019 Vdopia, Inc.

require 'json'

SCRIPT_DIR = File.expand_path(File.dirname(__FILE__))
RLS_TAGN = '_release_v'
AWS_LOCN = 's3://vdopia-sdk-files'
PKG_NAME = 'ChocolatePlatformAds.unitypackage'
VER_FILE = "#{SCRIPT_DIR}/sdk_versions.json"

def versioned_file(version)
  "#{PKG_NAME}#{RLS_TAGN}#{vers}"
end

## -- Running script --

vers = JSON.parse(File.read(VER_FILE))
exit if vers['ignore']

target = versioned_file(vers['version'])
%x(aws s3 cp #{PKG_NAME} #{AWS_LOCN}/#{target})
