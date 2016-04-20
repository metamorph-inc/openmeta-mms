import pygame.mixer
import time

filename="house_lo.ogg"
pygame.mixer.init()
f=open(filename,"rb")
pygame.mixer.music.load(f)
pygame.mixer.music.play(0)
time.sleep(1)
pygame.mixer.music.play(0)
time.sleep(1)