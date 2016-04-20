__author__ = 'ted bapty'

import sys
import pygame
from pygame.locals import *


def main(result_file):
    pygame.init()
    winSurfObj =pygame.display.set_mode((800,800))
    whiteColor = pygame.Color(255,255,255)
    blackColor = pygame.Color(0,0,0)
    greenColor = pygame.Color(128,255,128)
    pygame.display.set_caption('ChipFit -- Bin-Packed Board -- Esc to Quit, NumPad+/- to Zoom')
    i = 0
    j = 0
    x = 0
    y = 0
    fd = open(result_file, 'r')
    lines = fd.readlines()
    fd2 = open('layout-input.json')
    lines2 = fd2.readlines()
    cols2 = lines2[1].split(' ')
    print len(cols2)
    print cols2
    print cols2[3]
    print cols2[3].split(',')[0]
    print float(cols2[3].split(',')[0])
    boardwidth =  float(cols2[3].split(',')[0])
    #int(cols2[3])
    cols2 = lines2[2].split(' ')
    print len(cols2)
    print cols2
    print cols2[3].split(',')[0]
    print float(cols2[3].split(',')[0])
    boardheight = float(cols2[3].split(',')[0])
    #int(cols2[3])
    scale = 10
    refresh = 1
    badcount = 0
    font = pygame.font.SysFont("monospace",8)
    clock = pygame.time.Clock()
    while True:
        if refresh == 1:
            refresh = 0
            badcount = 0
            winSurfObj.fill(whiteColor)
            font = pygame.font.SysFont("monospace",scale)
            i = boardwidth*scale  + 4
            j = boardheight*scale + 4
            x = 0
            y = 0
            print boardwidth
            print boardheight
            print i 
            print j
            pygame.draw.rect(winSurfObj,greenColor,(x,y,i,j))
            for l in lines[1:]:
                cols = l.split(' ')
                print cols
                x = round(float(cols[2])*scale)
                y = round(float(cols[3])*scale)
                i = round(float(cols[4])*scale)
                j = round(float(cols[5])*scale)
                if x < 0:
                    if badcount < 1 :
                        x = boardwidth*scale + 4*scale
                        y = badcount * 2 * scale					
                        label = font.render('Failed To Place:', 1, (25,0,12))
                        winSurfObj.blit(label,(x+2,y+2))
                        badcount = badcount + 1
                    x = boardwidth*scale + 5*scale + 2
                    y = badcount * 1 * scale + 2
                    badcount = badcount + 1
                    label = font.render(cols[0], 1, (25,0,12))
                    winSurfObj.blit(label,(x+2,y+2))
                else :				
                    pygame.draw.rect(winSurfObj,blackColor,(x,y,i,j))
                    pygame.draw.rect(winSurfObj,whiteColor,(x+2,y+2,i-4,j-4))
                    label = font.render(cols[0], 1, (25,0,12))
                    winSurfObj.blit(label,(x+2,y+j/2-scale/2))
                pygame.display.update()

        clock.tick(30)
        for event in pygame.event.get():
            if event.type == QUIT:
                pygame.quit()
                sys.exit()
            elif event.type == KEYDOWN:
                if event.key == K_ESCAPE:
                    pygame.event.post(pygame.event.Event(QUIT))
                elif event.key == 269:
                    scale = scale - 1
                    if scale < 1:
                        scale = 1
                    refresh = 1
                elif event.key == 270:
                    scale = scale + 1
                    refresh = 1
