require 'pathname'
require 'rake/clean'

SELF_PATH = File.dirname(File.expand_path(__FILE__))
UNITYPROJ_PATH = File.expand_path("#{SELF_PATH}/../../../../")

TEXTURE_PACKER = '/Applications/TexturePacker.app/Contents/MacOS/TexturePacker'
HAS_TEXTURE_PACKER = File.exist?(TEXTURE_PACKER)
TEXTURE_PACKER_OPTS = '--size-constraints POT --algorithm MaxRects --maxrects-heuristics Best --scale 1.0 --pack-mode Best'
SWF2LWF_RB = File.expand_path("#{SELF_PATH}/swf2lwf/swf2lwf.rb")

#SWF_DIR ||= File.expand_path("#{UNITYPROJ_PATH}/../swf")
#LWF_DIR ||= File.expand_path("#{UNITYPROJ_PATH}/Assets/Resources/")
SWF_DIR ||= ""
LWF_DIR ||= ""

if SWF_DIR.empty? or LWF_DIR.empty?
  puts "ERROR: set 'SWF_DIR' and 'LWF_DIR'"
  exit 1
end

SWF_FILES_ABS = FileList["#{SWF_DIR}/**/*.swf"]
SWF_FILES_REL = SWF_FILES_ABS.map { |swf| swf.gsub(/#{SWF_DIR}(\/)?/, '') }
SWF_FILES = SWF_FILES_ABS.zip(SWF_FILES_REL)


#p SWF2LWF_RB
#p File.exist?(SWF2LWF_RB)
#p SWF_DIR
#p LWF_DIR
p SWF_FILES

task :hogehoge do
  p SELF_PATH
  p SWF2LWF_RB
end

namespace :lwf do
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
    bmpdir = swf.pathmap('%X.bitmap')
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
    CLEAN.include(lwfdir, bmpdir)
    directory lwfdir
    directory bmpdir
    file bmp => [swf, lwfdir, bmpdir] do |t| # extract texture and generate atlus
      sh "ruby #{SWF2LWF_RB} -p #{t.prerequisites[0]}"
      texs = FileList["#{t.prerequisites[1]}/*.png"]
      sh "#{TEXTURE_PACKER} #{TEXTURE_PACKER_OPTS} --format json --data #{json} --sheet #{bmp} #{texs}"
      rm "#{lwf}"
    end
    file lwf => [swf, bmp] do |t| #
      if File.exist?(fla)
        sh "ruby #{SWF2LWF_RB} -f #{fla} #{swf} #{json}"
      else
        sh "ruby #{SWF2LWF_RB} #{swf} #{json}"
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

  #p LWFDATA_DIRS
  #p BMP_FILES
  #p LWF_FILES
  #p ROMBMP_FILES
  #p ROMLWF_FILES


  desc "extract bitmap from swf"
  task :bmp => BMP_FILES
  desc "convert swf to lwf"
  task :conv => LWF_FILES
  desc "do bmp, conv"
  task :build => [:bmp, :conv]
  desc "install converted lwf and png"
  task :install => ROMLWF_FILES + ROMBMP_FILES
end

__END__
BITMAP_ATLUSES = FileList.new
BITMAP_DIRS.each do |d|
  #p "hoge #{File.basename(d)}"
  if Dir.exist?(d)
    atlus = "#{TMPDIR}/#{File.basename(d)}.png"
    json = "#{TMPDIR}/#{File.basename(d)}.json"
    texs = FileList["#{d}/*.png"]
    BITMAP_ATLUSES.include(atlus)
    file atlus => texs do |t|
      sh "#{TEXTURE_PACKER} --scale 1.0 --format json --data #{json} --sheet #{atlus} #{texs}"
    end
    CLEAN.include(atlus)
  end
end

LWF_FILES = FileList.new
ROM_FILES = FileList.new
SWF_FILES.each do |swf|
  #p swf
  lwf_dir = swf.pathmap("%X.lwfdata")
  lwf = swf.pathmap("#{lwf_dir}/%X.lwf")
  lwf_rom = swf.pathmap("#{ROMDIR}/%X.bytes")
  #p lwf
  src = FileList.new
  src << swf
  atlus_tmp = swf.pathmap("#{TMPDIR}/%X.bitmap.png")
  atlus_rom = swf.pathmap("#{ROMDIR}/%X.bitmap.png")
  json = swf.pathmap("#{TMPDIR}/%X.bitmap.json")
  src << atlus_tmp if BITMAP_ATLUSES.include?(atlus_tmp)
  #p "LWF #{lwf}"
  #p "SRC #{src}"
  LWF_FILES.include(lwf)
  file lwf => src do |t|
    sh "ruby #{SWF2LWF_RB} #{swf} #{json}"
  end
  unless ROMDIR.empty?
    p ROMDIR
    #LWF_FILES.each do
    if BITMAP_ATLUSES.include?(atlus_tmp)
      file atlus_rom => atlus_tmp do |t|
        sh "cp #{t.prerequisites[0]} #{t.name}"
        p t
      end
      ROM_FILES << atlus_rom
    end
    file lwf_rom => lwf do |t|
      sh "cp #{t.prerequisites[0]} #{t.name}"
      p t.name
      p t.prerequisites
    end
    ROM_FILES << lwf_rom
  end
  CLEAN.include(lwf_dir)
end



