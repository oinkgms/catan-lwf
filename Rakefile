PACKAGE_NAME = 'catan-lwf'

require './package'

PROJECT_DLL_PATH = "#{PROJECT_PATH}/Assets/#{PLUGINS_ROOT}"
SOLUTION_NAME = 'build-dll'
SOLUTION_FILE = "#{SOLUTION_NAME}/#{SOLUTION_NAME}.sln"
OUTPUT_DLL_PATH = "#{SOLUTION_NAME}/bin"
DEBUG_DLL = "#{OUTPUT_DLL_PATH}/Debug/UniLua.dll"
DEBUG_MDB = "#{DEBUG_DLL}.mdb"
RELEASE_DLL = "#{OUTPUT_DLL_PATH}/Release/UniLua.dll"

CLEAN.include(OUTPUT_DLL_PATH)

APIGEN_RB = "ruby tool/apigen.rb"
CS_SRCS = FileList['unityproj/**/*.cs']

task :default do
  sh "rake -T"
end
desc "build UniLua.dll"
task :build_dll do
  sh "#{MDTOOL} build '--configuration:Debug' #{SOLUTION_FILE}"
  sh "#{MDTOOL} build '--configuration:Release' #{SOLUTION_FILE}"
end
desc "install debug .DLL"
task :install_debug => [:build_dll] do
  install DEBUG_DLL, PROJECT_DLL_PATH
  install DEBUG_MDB, PROJECT_DLL_PATH
end
desc "install release .DLL"
task :install_release => [:build_dll] do
  install RELEASE_DLL, PROJECT_DLL_PATH
end
desc "ganerate API reference document"
task :gendoc do
  sh "#{APIGEN_RB} -o doc/index.html -m doc/index.md #{CS_SRCS}"
end
