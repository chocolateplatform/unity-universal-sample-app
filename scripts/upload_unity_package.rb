#!/usr/bin/env ruby

# Author: Lev Trubov
# © 2019 Vdopia, Inc.

require 'json'

SCRIPT_DIR = File.expand_path(File.dirname(__FILE__))
RLS_TAGN = '_v'
AWS_LOCN = 's3://vdopia-sdk-files'
PKG_NAME = 'ChocolatePlatformAds'
PKG_EXTN = '.unitypackage'
PKG_FILE = "#{PKG_NAME}#{PKG_EXTN}"
VER_FILE = "#{SCRIPT_DIR}/versions.json"

def versioned_file(version)
  "#{PKG_NAME}#{RLS_TAGN}#{version}#{PKG_EXTN}"
end

## -- Running script --

vers = JSON.parse(File.read(VER_FILE))
exit if vers['ignore']

target = versioned_file(vers['plugin-unity'])
%x(aws s3 cp #{PKG_FILE} #{AWS_LOCN}/#{target})
