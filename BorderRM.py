
# ___  ___  ___  ___  ________  ________     
#|\  \|\  \|\  \|\  \|\   ____\|\   __  \    
#\ \  \\\  \ \  \\\  \ \  \___|\ \  \|\  \   
# \ \   __  \ \  \\\  \ \  \  __\ \  \\\  \  
#  \ \  \ \  \ \  \\\  \ \  \|\  \ \  \\\  \ 
#   \ \__\ \__\ \_______\ \_______\ \_______\
#    \|__|\|__|\|_______|\|_______|\|_______|
#                                            
#    26-5-2020 22:30: CHANGE: ASCII ART
#
#   TODO:
#       Interface voor BorderRM,
#       Maak samenhang van BorderRM, Collage, en GREY2WHITE
#
#
#
#
import numpy as np
import cv2
import time, sys
from PIL import Image, ImageChops
import os

if os.path.exists("./input/"):
    print("input is er")
else:
    os.mkdir("./input/")
if os.path.exists("./output/"):
    print("output is er")
else:
    os.mkdir("./output/")

filearray = []
def trim(im):
    try:
        img = Image.open("./input/" + im)
        
        bg = Image.new(img.mode, img.size, img.getpixel((0,0)))
        diff = ImageChops.difference(img, bg)
        diff = ImageChops.add(diff, diff, 2.0, -100)
        bbox = diff.getbbox()
        os.remove("./input/" + im)
        return img.crop(bbox)
        

    except:
        print("kan " + str(im) + " niet doen" )

for geladenfile in os.listdir("./input"):
    a = trim(geladenfile)
    print(geladenfile)
    try:
        a.save("./output/" + geladenfile)

    except:
        print(" Kan " + geladenfile + " niet opslaan")
i = True