#!/usr/bin/env ruby

# Author: Lev Trubov
# © 2019 Vdopia, Inc.

PROJ_DIR = 'SampleApp'
UNITY_EX = '/Applications/Unity/Unity.app/Contents/MacOS/Unity'
PKG_NAME = 'ChocolatePlatformAds.unitypackage'

def build
  cmd = "(cd #{PROJ_DIR} && "
  cmd += "#{UNITY_EX} -nographics -batchmode -quit -projectPath `pwd` "
  cmd += "-exportPackage Assets/Plugins Assets/Scripts Assets/Editor "
  cmd += "#{ENV['TMPDIR']}#{PKG_NAME}"
  cmd += ")"
  puts cmd
  system(cmd)
end

## -- Running script --

build
