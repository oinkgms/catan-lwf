PACKAGE_NAME = 'catan-lwf'
ADDITIONAL_EXPORT_PATH = 'Assets/Resources/shaders'

require './subtools/package'
require './unityproj/Assets/Editor/catan-lwf/Tools/conv_lwf'

PROJECT_LWF_DLL_PATH = "#{PROJECT_PATH}/Assets/#{PLUGINS_ROOT}"
PROJECT_LWF_EDITOR_DLL_PATH = "#{PROJECT_PATH}/Assets/#{EDITOR_ROOT}"
BASE_LWF_PATH = "base"
BUILD_PATH = "#{BASE_LWF_PATH}/csharp/unity/build"
LWF_DLL = "#{BUILD_PATH}/lwf.dll"
LWF_EDITOR_DLL = "#{BUILD_PATH}/lwf_editor.dll"

#CLEAN.include(OUTPUT_DLL_PATH)

APIGEN_RB = "ruby tool/apigen.rb"
CS_SRCS = FileList['unityproj/**/*.cs']

task :default do
  sh "rake -T"
end
desc "build lwf.dll"
task :build_dll do
  cd "base/csharp/unity/build" do
    sh "rake"
  end
end
desc "install lwf.dll"
task :install_dll => [:build_dll] do
  install LWF_DLL, PROJECT_LWF_DLL_PATH
  install LWF_EDITOR_DLL, PROJECT_LWF_EDITOR_DLL_PATH
end
desc "ganerate API reference document"
task :gendoc do
  sh "#{APIGEN_RB} -o doc/index.html -m doc/index.md #{CS_SRCS}"
end
