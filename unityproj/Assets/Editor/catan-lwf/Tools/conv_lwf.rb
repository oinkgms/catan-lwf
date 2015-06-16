require 'pathname'
require 'rake/clean'

SELF_PATH = File.dirname(File.expand_path(__FILE__))
UNITYPROJ_PATH = File.expand_path("#{SELF_PATH}/../../../../")

TEXTURE_PACKER = '/Applications/TexturePacker.app/Contents/MacOS/TexturePacker'
HAS_TEXTURE_PACKER = File.exist?(TEXTURE_PACKER)
TEXTURE_PACKER_OPTS = '--force-squared --size-constraints POT --algorithm MaxRects --maxrects-heuristics Best --scale 1.0 --pack-mode Best --texture-format png --png-opt-level 1 --opt RGBA4444 --max-size 2048 --scale-mode Smooth --trim-mode None --trim-threshold 1 --border-padding 2 --shape-padding 2 --inner-padding 0 --common-divisor-x 1 --common-divisor-y 1 --reduce-border-artifacts --dither-fs-alpha'
SWF2LWF_RB = File.expand_path("#{SELF_PATH}/swf2lwf/swf2lwf.rb")

#SWF_DIR ||= File.expand_path("#{UNITYPROJ_PATH}/../swf")
#LWF_DIR ||= File.expand_path("#{UNITYPROJ_PATH}/Assets/Resources/")
SWF_DIR ||= ""
LWF_DIR ||= ""

if SWF_DIR.empty? or LWF_DIR.empty?
  puts "ERROR: set 'SWF_DIR' and 'LWF_DIR'"
  exit 1
end
unless HAS_TEXTURE_PACKER
  puts "ERROR: you don't have a TexturePacker"
  exit 1
end

SWF_FILES_ABS = FileList["#{SWF_DIR}/**/*.swf"]
SWF_FILES_REL = SWF_FILES_ABS.map { |swf| swf.gsub(/^#{SWF_DIR}(\/)?/, '') }
SWF_FILES = SWF_FILES_ABS.zip(SWF_FILES_REL)


#p SWF2LWF_RB
#p File.exist?(SWF2LWF_RB)
#p SWF_DIR
#p LWF_DIR
#p SWF_FILES


LWFDATA_DIRS = FileList.new
BMP_FILES = FileList.new
LWF_FILES = FileList.new
ROMLWF_FILES = FileList.new
ROMBMP_FILES = FileList.new
SWF_FILES.each do |pairs|
  swf = pairs[0]
  rel = pairs[1]
  fla = swf.pathmap('%X.fla')
  lwfdir = swf.pathmap('%X.lwfdata')
  bmpdir = swf.pathmap('%X.bitmap') # by Publish for LWF
  publwf = File.exist?(bmpdir)
  unless publwf
    bmpdir = swf.pathmap('%X.ext_bitmap') # extract by swf2lwf
  end
  bmp = swf.pathmap("#{bmpdir}/%n.bitmap.png")
  json = swf.pathmap("#{bmpdir}/%n.bitmap.json")
  lwf = swf.pathmap("#{lwfdir}/%n.lwf")
  romlwf = rel.pathmap("#{LWF_DIR}/%X.bytes")
  rombmp = rel.pathmap("#{LWF_DIR}/%X.bitmap.png")
  romdir = File.dirname(romlwf)
  LWFDATA_DIRS.include(lwfdir)
  BMP_FILES.include(bmp)
  LWF_FILES.include(lwf)
  ROMLWF_FILES.include(romlwf)
  ROMBMP_FILES.include(rombmp)
  CLEAN.include(lwfdir)
  unless publwf
    CLEAN.include(bmpdir)
  else
    CLEAN.include(bmp, json)
  end
  directory lwfdir
  directory bmpdir
  namespace :lwf do
    if publwf
      # use publish for lwf
      texsrc = FileList["#{bmpdir}/*.png"].exclude(bmp)
      file bmp => [swf, lwfdir] + texsrc do |t|
        sh "#{TEXTURE_PACKER} #{TEXTURE_PACKER_OPTS} --format json --data #{json} --sheet #{bmp} #{texsrc}"
      end
    else
      # extract texture and generate atlas
      file bmp => [swf, lwfdir, bmpdir] do |t|
        sh "ruby #{SWF2LWF_RB} -p #{t.prerequisites[0]}"
        texsrc = FileList["#{t.prerequisites[1]}/*.png"]
        sh "#{TEXTURE_PACKER} #{TEXTURE_PACKER_OPTS} --format json --data #{json} --sheet #{bmp} #{texsrc}"
        rm "#{lwf}"
      end
    end
  end
  lwftask = File.dirname(swf) + '/' + File.basename(swf, ".*")
  desc "Convert #{swf}"
  task lwftask => [romlwf, rombmp]
  #task lwftask => [bmp]
  namespace :lwf do
    file lwf => [swf, bmp] do |t| #
      if File.exist?(fla)
        sh "ruby #{SWF2LWF_RB} -s -f #{fla} #{swf} #{json}"
      else
        sh "ruby #{SWF2LWF_RB} -s #{swf} #{json}"
      end
    end
    directory romdir
    file romlwf => [romdir, lwf] do |t|
      sh "cp #{t.prerequisites[1]} #{t.name}"
    end
    file rombmp => [romdir, bmp] do |t|
      sh "cp #{t.prerequisites[1]} #{t.name}"
    end
  end
end

#p LWFDATA_DIRS
#p BMP_FILES
#p LWF_FILES
#p ROMBMP_FILES
#p ROMLWF_FILES

namespace :lwf do
  desc "extract bitmap from swf"
  task :bmp => BMP_FILES
  desc "convert swf to lwf"
  task :conv => LWF_FILES
  desc "do bmp, conv"
  task :build => [:bmp, :conv]
  desc "install converted lwf and png"
  task :install => ROMLWF_FILES + ROMBMP_FILES
end

