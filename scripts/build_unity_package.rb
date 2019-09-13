#!/usr/bin/env ruby

# Author: Lev Trubov
# © 2019 Vdopia, Inc.

PROJ_DIR = 'SampleApp'
UNITY_DIR = '/Applications/Unity/Hub/Editor/'
UNITY_VER = '2019.2.5f1'
UNITY_EX_PATH = '/Unity.app/Contents/MacOS/Unity'
UNITY_EX = "#{UNITY_DIR}#{UNITY_VER}#{UNITY_EX_PATH}"
PKG_NAME = 'ChocolatePlatformAds.unitypackage'

def build
  cmd = "(cd #{PROJ_DIR} && "
  cmd += "#{UNITY_EX} -nographics -batchmode -quit -projectPath `pwd` "
  cmd += "-exportPackage Assets/Plugins Assets/ChocolateMediation "
  cmd += "#{PKG_NAME}"
  cmd += ")"
  puts cmd
  system(cmd)
end

## -- Running script --

build
