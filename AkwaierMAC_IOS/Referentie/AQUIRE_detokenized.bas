    5 IF PAGE<>&1900 PAGE=&1900:CHAIN"AQUIRE"
   10 info$="aquire 23/05/87"
   11 DIMkleinere(4)
   12 DIMspeelsteen%(8,6)
   13 DIMprijs_keten%(8)
   14 DIMaandelen%(8,6)
   15 DIMbezit%(6),f(6)
   16 DIMarray%(36),array$(36),arraypo%(36),arraynop%(8)
   17 DIMaandeelsort%(8,6)
   18 DIMvermogen%(8),SPELER%(6),speler$(6)
   19 *TV0,1
   20 MODE6
   22 doe%=FALSE
   23 DIM nop%(2,8)
   25 DIM macht%(9)
   30 FOR N%=0 TO 8:macht%(N%)=2^N%:NEXT
   55 VDU23,1,0;0;0;0;
   56 W%=0
   60 kiezen%=FALSE
   61 sticht%=255
   65 @%=0
  100 FORN%=1TO15
  110 FORM%=1TO15
  120 PROCpoke_keten(N%,M%,-1)
  130 NEXT:NEXT
  131 RESTORE310
  140 FORN%=1TO6
  145 READspeler$(N%)
  150 bezit%(N%)=25000
  155 SPELER%(N%)=N%
  160 NEXT
  300 GOTO2000
  310 DATA"OPA","OMA","BROER","ZUS","PA","MA" 
  999 ::::::::::::::::::::::::::
 1000 DEFPROCbord_uitprinten
 1010 CLS
 1015 COLOUR0:COLOUR134
 1020 PRINT" 123456789012345 "
 1025 COLOUR3:COLOUR128
 1035 PROCbereken_prijs_keten
 1040 FORN%=1TO15
 1045 COLOUR0:COLOUR134
 1050 PRINTCHR$(N%+64);
 1055 COLOUR3:COLOUR128
 1060 FORM%=1TO15
 1070 O%=FNketen(M%,N%)
 1080 IFO%=-1 VDU249; ELSE IFO%=0 COLOUR6:VDU176;:COLOUR3 ELSE RESTORE(28999+O%):READAA%,BB%:COLOURAA%:COLOURBB%:PRINTCHR$(O%+&40);
 1081 COLOUR128:COLOUR3
 1090 NEXT
 1100 COLOUR0:COLOUR134:PRINTCHR$(N%+64):COLOUR3:COLOUR128
 1110 NEXT
 1115 COLOUR0:COLOUR134:PRINT" 123456789012345 ":COLOUR3:COLOUR128
 1120 ENDPROC
 1124 ::::::::::::::::::::::::::
 1125 DEFPROCprint_geld
 1126 PROCvermogen
 1130 tabx%=20:taby%=19
 1135 COLOUR0:COLOUR130:PRINTTAB(tabx%,taby%-1)"  bezit  vermogen  ":COLOUR3:COLOUR128       
 1140 FORN%=1TO6
 1145 PRINTTAB(tabx%,taby%)(N%);
 1146 PRINTTAB(tabx%+2,taby%)bezit%(N%);"  ";
 1147 PRINTTAB(tabx%+10,taby%)vermogen%(N%)" ";  
 1150 taby%=taby%+1
 1155 NEXT
 1160 ENDPROC
 1199 ::::::::::::::::::::::::::
 1200 DEFPROCcontroleer_aanliggen(xpos%,ypos%)
 1201 aanliggen%=FALSE:LOCAL R%
 1204 FORN%=0TO3:kleinere(N%)=0:NEXT
 1205 MAX%=0:kleur%=0:kanniet%=0:S%=0:gesticht%=TRUE
 1206 RESTORE1300
 1207 kleinere(0)=FNketen(xpos%,ypos%)
 1209 FORTT%=1TO2
 1210 RESTORE1300
 1211 FORN%=1TO4
 1212 READdatax%,datay%
 1213 X%=xpos%+datax%:Y%=ypos%+datay%
 1214 R%=FNketen(X%,Y%)
 1215 IF X%>15 OR X%<1 OR Y%>15 OR Y%<1 GOTO1230
 1220 IF(R%>0 AND R%<10) M%=R%:PROCkies_grootste:aanliggen%=TRUE
 1223 plus%=0
 1224 IF(R%=0 AND aanliggen%=TRUE) kanniet%=FALSE:IF(doe%=TRUE AND TT%=2) PROCsteen_leggen(X%,Y%,10)
 1225 IF((R%=0 OR R%=10) AND aanliggen%=FALSE) kanniet%=TRUE
 1230 NEXT
 1235 NEXT
 1240 ENDPROC
 1241 ::::::::::::::::::::::::::
 1250 DEFPROCkies_grootste
 1251 IF TT%=1 ENDPROC
 1255 IF(FNmax(M%)=MAX% AND FNmax(M%)<>0 AND M%<>kleur%) kiezen%=TRUE
 1260 IF(FNmax(M%)>MAX% AND M%<>10) MAX%=FNmax(M%):kleur%=M% : kiezen%=FALSE
 1270 S%=S%+1:kleinere(S%)=M%
 1280 ENDPROC
 1281 ::::::::::::::::::::::::::
 1300 DATA1,0 
 1310 DATA-1,0 
 1320 DATA0,1      
 1330 DATA0,-1        
 1331 ::::::::::::::::::::::::::
 1400 DEFPROCverander_na_fusie
 1401 LOCAL H%
 1405 PROCverdeel_geld
 1410 FORN%=1TO15
 1420 FORM%=1TO15:H%=FNketen(N%,M%)
 1421 IF H% =-1 GOTO1450
 1422 IF H% =kleur% GOTO1450
 1430 FORsa%=0TOS%
 1432 IF H% =kleur% GOTO1440
 1435 IF H% =kleinere(sa%) PROCsteen_leggen(N%,M%,kleur%)
 1440 NEXT
 1450 NEXT
 1455 NEXT
 1460 sticht%=0
 1465 FORN%=0TO7
 1466 IFFNmax(N%+1)=0 sticht%=sticht%+macht%(N%)
 1467 NEXT
 1490 ENDPROC
 1491 ::::::::::::::::::::::::::
 1500 DEFPROCsteen_leggen(xpos%,ypos%,kleur%)
 1505 @%=0
 1510 IFkleur%=-1 PRINTTAB(xpos%,ypos%)CHR$249
 1520 IFkleur%=0 COLOUR2:PRINTTAB(xpos%,ypos%)CHR$176::COLOUR3:GOTO1535
 1525 IFkleur%=10 COLOUR7:PRINTTAB(xpos%,ypos%)CHR$219:GOTO1531
 1530 IFkleur%>0 RESTORE(28999+kleur%):READAA%,BB%:COLOURAA%:COLOURBB%:PRINTTAB(xpos%,ypos%)CHR$(kleur%+&40)      
 1531 COLOUR128:COLOUR3  
 1535 PROCpoke_keten(xpos%,ypos%,kleur%)
 1536 PROCpoke_max(kleur%,plus%)
 1540 ENDPROC
 1541 ::::::::::::::::::::::::::
 1600 DEFPROCsteen_pakken
 1601 IF?&900<=1 steen%=-16:ENDPROC
 1605 LOCALM%,N%
 1610 N%=RND(?&900):steen%=?(&900+N%)
 1629 IFN%<(?&900) ?(&900+N%)=?(&900+(?&900))
 1630 ?&900=(?&900)-1
 1640 PRINTTAB(0,20)"Stenen over "?&900"  "
 1680 ENDPROC
 1681 ::::::::::::::::::::::::::
 1700 DEFPROCstenen_initieeren
 1710 FORN%=1TO225
 1715 ?(&900+N%)=N%
 1720 NEXT
 1730 ?(&900)=N%-1
 1731 FORN%=0TO10:?(&B30+N%)=0:NEXT
 1740 ENDPROC
 1741 ::::::::::::::::::::::::::
 1750 DEFPROCinformatie_ketens
 1751 PROCbereken_prijs_keten
 1753 tabx%=20:taby%=1
 1754 COLOUR132:COLOUR3:PRINTTAB(tabx%,taby%-1)"   prijs  aantal   "   
 1755 FORN%=1TO8
 1757 RESTORE(28999+N%):READAA%,BB%:COLOURAA%:COLOURBB%
 1760 PRINTTAB(tabx%,taby%)CHR$(N%+64);:COLOUR128:COLOUR3:PRINT"  "prijs_keten%(N%)"   "
 1765 taby%=taby%+1
 1790 NEXT
 1792 COLOUR128:COLOUR3
 1795 ENDPROC
 1796 ::::::::::::::::::::::::::
 1800 DEFPROCinformatie_max
 1810 tabx%=32:taby%=1  
 1820 FORN%=1TO8
 1830 PRINTTAB(tabx%,taby%)FNmax(N%)"  "
 1840 taby%=taby%+1
 1850 NEXT
 1860 ENDPROC
 1861 ::::::::::::::::::::::::::
 1900 DEFPROCbereken_prijs_keten
 1905 RESTORE20000
 1910 FORN%=1TO8:READprijs_keten%(N%):NEXT
 1915 FORN%=1TO8
 1920 RESTORE1980
 1925 IFFNmax(N%)=0 prijs_keten%(N%)=0
 1930 FORM%=1TO10
 1935 READP%:IFFNmax(N%)>P% prijs_keten%(N%)=prijs_keten%(N%)+100
 1940 NEXT
 1945 K%=0
 1950 FORM%=1TO6
 1955 K%=K%+aandelen%(N%,M%)
 1960 NEXT
 1965 prijs_keten%(N%)=prijs_keten%(N%)+K%*10
 1970 NEXT
 1975 ENDPROC
 1976 ::::::::::::::::::::::::::
 1980 DATA0,1,2,3,4,9,14,19,24,35
 1981 ::::::::::::::::::::::::::
 2000 DEFPROCstart_programma
 2001 A=0:A$=""
 2005 *FX15,1
 2009 PRINT"DOE JE MEE MENS ?":PRINT:PRINT"DRUK EEN TOETS"
 2010 Z$=INKEY$(50)   
 2011 IFZ$<>"" SPELER%=RND(6) ELSE SPELER%=0:GOTO2015
 2012 PRINT:PRINT"MENS JE BENT NR "SPELER%
 2013 INPUT"Naam?"A$
 2015 PROCstenen_initieeren
 2020 PROCbord_uitprinten
 2021 PROCinformatie_ketens
 2022 PROCinformatie_max
 2023 PROCinfo_aandelen
 2024 speler$(SPELER%)=LEFT$(A$,6)
 2025 PROCprint_geld
 2030 PROCinformatie_ketens
 2035 PROCstenen_initieeren
 2040 FORN%=1TO8
 2045 FORM%=1TO6
 2046 PROCsteen_pakken
 2050 speelsteen%(N%,M%)=steen%
 2053 NEXT
 2054 PRINT
 2055 NEXT
 2060 A$=STRING$(20," ")
 2064 IFSPELER%>0 mens%=TRUE:PROCprint_stenen(SPELER%)
 2070 PROCspelen
 2080 GOTO2070
 2081 ::::::::::::::::::::::::::::::
 2200 DEFPROCspeelsteen_uit_array_nemen(G%,speler%)
 2210 speelsteen%=speelsteen%(G%,speler%)
 2215 IFspeelsteen%=-16 steen%=-16:ENDPROC
 2240 ypos%=(speelsteen%MOD15):IFypos%=0ypos%=15
 2250 xpos%=speelsteen%DIV15:IFxpos%=0xpos%=15
 2260 kleur%=0
 2270 ENDPROC
 2271 ::::::::::::::::::::::::::
 2300 DEFPROCnieuwe_speelsteen_in_array(G%,speler%)
 2310 PROCsteen_pakken
 2320 speelsteen%(G%,speler%)=steen%
 2340 ENDPROC
 2341 ::::::::::::::::::::::::::
 2400 DEFPROClege_steen_terug(xpos%,ypos%)
 2410 RESTORE1300
 2415 PROCsteen_leggen(xpos%,ypos%,-1)
 2420 FORN%=1TO4
 2425 READdatax%,datay%
 2430 IFFNketen(xpos%+datax%,ypos%+datay%)=10 PROCsteen_leggen(xpos%,ypos%,-1)
 2440 NEXT
 2450 ENDPROC
 2451 ::::::::::::::::::::::::::
 2500 DEFPROCcontroleer_vrij(xpos%,ypos%)
 2510 RESTORE1300
 2515 gesticht%=TRUE
 2520 FORN%=1TO4
 2530 READdatax%,datay%
 2535 IFxpos%+datax%>15 OR xpos%+datax%<1 OR ypos%+datay%>15 OR ypos%+datay%<1 THEN GOTO2550
 2540 IFFNketen(xpos%+datax%,ypos%+datay%)>=0 gesticht%=FALSE:N%=5
 2550 NEXT
 2560 ENDPROC
 2561 ::::::::::::::::::::::::::
 2600 DEFPROCinfo_aandelen
 2610 tabx%=20:taby%=10
 2619 COLOUR0:COLOUR133
 2620 PRINTTAB(tabx%,taby%-1)"  1  2  3  4  5  6 "
 2621 COLOUR3:COLOUR128
 2630 FORN%=1TO8
 2635 RESTORE(28999+N%):READAA%,BB%:COLOURAA%:COLOURBB%     
 2640 PRINTTAB(tabx%,taby%)CHR$(N%+64)
 2642 COLOUR3:COLOUR128
 2645 tabx%=tabx%+2
 2646 FORM%=1TO6
 2649 IFaandelen%(N%,M%)=0 GOTO2655
 2650 IF(aandelen%(N%,M%)=aandeelsort%(N%,1)DIV10) COLOUR7:COLOUR134
 2651 IF(aandelen%(N%,M%)=aandeelsort%(N%,2)DIV10) COLOUR7:COLOUR133
 2655 PRINTTAB(tabx%,taby%)aandelen%(N%,M%);
 2656 COLOUR3:COLOUR128
 2657 PRINT" "
 2660 tabx%=tabx%+3
 2665 NEXT
 2670 tabx%=20:taby%=taby%+1
 2675 NEXT
 2690 ENDPROC
 2699 ::::::::::::::::::::::::::
 2700 DEFPROCverdeel_geld
 2710 FORN%=1TO6:array%(N%)=0:NEXT:Q%=FALSE
 2720 FORsa%=0TOS%
 2725 IF(kleinere(sa%)<>kleur% AND kleinere(sa%)<9) weg%=kleinere(sa%):IF FNmax(weg%)>0 Q%=TRUE:PROCverdeel
 2730 NEXT
 2731 IFQ%=FALSE ENDPROC
 2735 taby%=19
 2740 FORN%=1TO6
 2745 PRINTTAB(0,taby%)"speler "N%"  "array%(N%);
 2750 taby%=taby%+1
 2755 NEXT
 2770 ENDPROC
 2799 ::::::::::::::::::::::::::
 2800 DEFPROCverdeel
 2801 PROCwis
 2805 A%=aandeelsort%(weg%,1)DIV10:B%=aandeelsort%(weg%,2)DIV10:C%=aandeelsort%(weg%,3)DIV10:D%=aandeelsort%(weg%,4)DIV10:E%=aandeelsort%(weg%,5)DIV10:Z6%=aandeelsort%(weg%,6)DIV10
 2806 tot%=FNmax(weg%)*prijs_keten%(weg%)
 2807 E%=0:FORN%=1TO6:E%=E%+aandelen%(weg%,N%):aandelen%(kleur%,N%)=aandelen%(kleur%,N%)+aandelen%(weg%,N%)/2:aandelen%(weg%,N%)=0:NEXT
 2808 tot%=tot%+E%*prijs_keten%(weg%)
 2810 IFB%=0f(1)=1:f(2)=0:f(3)=0:f(4)=0:f(5)=0:f(6)=0:GOTO2820
 2811 IF(A%=B% AND A%=C%) f(1)=.33:f(2)=.33:f(3)=.33:f(4)=0:f(5)=0:f(6)=0:GOTO2820
 2812 IF(B%=C% AND C%=D% AND D%=E% AND E%=Z6%) f(1)=.66:f(2)=.061:f(3)=.061:f(4)=.061:f(5)=.061:f(6)=.061:GOTO2820
 2813 IF(B%=C% AND C%=D% AND D%=E%) f(1)=.66:f(2)=.085:f(3)=.085:f(4)=.085:f(5)=.085:f(6)=0:GOTO2820
 2814 IFB%=C% f(1)=.66:f(2)=.17:f(3)=.17:f(4)=0:f(5)=0:f(6)=0:GOTO2820
 2815 IFA%=B% f(1)=.5:f(2)=.5:f(3)=0:f(4)=0:f(5)=0:f(6)=0:GOTO2820
 2816 IF(B%=C% AND C%=D%) f(1)=.66:f(2)=.11:f(3)=.11:f(4)=.11:f(5)=0:f(6)=0:GOTO2820
 2819 f(1)=.66:f(2)=.33:f(3)=0:f(4)=0:f(5)=0:f(6)=0
 2820 FORN%=1TO6
 2830 bezit%(aandeelsort%(weg%,N%)MOD10)=bezit%(aandeelsort%(weg%,N%)MOD10)+f(N%)*tot%
 2835 array%(aandeelsort%(weg%,N%)MOD10)=array%(aandeelsort%(weg%,N%)MOD10)+f(N%)*tot%
 2840 NEXT
 2845 ?(&B30+weg%)=0
 2850 ENDPROC
 2899 ::::::::::::::::::::::::::
 2900 DEFPROCvermogen
 2910 FORN%=1TO6
 2911 T%=0
 2915 FORM%=1TO8
 2920 T%=T%+prijs_keten%(M%)*aandelen%(M%,N%)
 2925 NEXT
 2930 vermogen%(N%)=bezit%(N%)+T%
 2940 NEXT
 2960 ENDPROC
 2999 ::::::::::::::::::::::::::
 3000 DEFPROCsteen_bepalen(speler%)
 3100 IFspeler%=SPELER% mens%=TRUE ELSE mens%=FALSE
 3105 IFmens%=FALSE PROCcomp(speler%):ENDPROC
 3106 H%=FALSE:*FX15,1
 3107 PROCwis:PRINTTAB(0,22)"Speler "speler%"  "speler$(speler%)"   "
 3110 PROCprint_stenen(speler%)
 3111 IFsticht%>0 PRINTTAB(0,23)"STICHTEN? [J/N]" ELSE GOTO3115
 3112 REPEAT:C$=GET$:UNTILINSTR("jJnN",C$)>0
 3113 IFINSTR("jJnN",C$)<3 H%=TRUE ELSE H%=FALSE
 3115 PRINTTAB(0,23)"                  ":PRINTTAB(0,20)"NR steen,0 kan niet"
 3119 *FX15,1
 3120 j%=GET
 3140 j%=j%-48
 3150 IFj%=0 kanniet%=kanniet%+1:ENDPROC
 3155 IFj%<1 OR j%>8 VDU7,7,7:GOTO3120 
 3160 PROCspeelsteen_uit_array_nemen(j%,speler%)
 3161 IF H% PRINTTAB(0,23)"Welke kleur ?  ";: ELSE GOTO3174
 3162 REPEAT:C$=GET$:UNTILINSTR("ABCDEFGH",C$)>0
 3164 kl%=INSTR("ABCDEFGH",C$):PRINTC$
 3167 IFFNmax(kl%)>0 VDU7:GOTO3115
 3171 IF(sticht%>0 AND H%=TRUE) PROCstichten:IFgesticht%=TRUE aandelen%(kleur%,speler%)=aandelen%(kleur%,speler%)+1:GOTO3204
 3173 IFgesticht%=FALSE VDU7:GOTO3115
 3174 plus%=0
 3175 PROCsteen_leggen(xpos%,ypos%,10)
 3180 PROCcontroleer_aanliggen(xpos%,ypos%)
 3183 doe%=FALSE
 3190 IFkanniet%=TRUE PROClege_steen_terug(xpos%,ypos%):GOTO3110
 3200 IFaanliggen%=TRUE AND kiezen%=TRUE:PRINTTAB(0,20)"Welke keten"'"als grootste?":G%=GET-&40:IFG%<0ORG%>8VDU7:GOTO3200ELSEkleur%=G%
 3201 PROCsteen_leggen(xpos%,ypos%,10)
 3202 doe%=TRUE:PROCcontroleer_aanliggen(xpos%,ypos%)
 3204 plus%=1
 3205 PROCsteen_leggen(xpos%,ypos%,kleur%)
 3206 PROCnieuwe_speelsteen_in_array(j%,speler%)
 3210 IFaanliggen%=TRUE PROCverander_na_fusie
 3220 PROCinformatie_max
 3225 PROCprint_geld
 3230 PROCinformatie_ketens
 3231 PROCwis:koopt%=0
 3232 PRINTTAB(0,20)"HOEVEEL KOPEN?":Z$=GET$:koopaantal%=VALZ$:IF koopaantal%>0 PRINT"WELKE KLEUR?":B$=GET$: ELSE GOTO3240
 3233 koopt%=ASCB$-&40:IFkoopt%<1ORkoopt%>8PRINTTAB(0,20)A$'A$:VDU7:GOTO3232
 3234 IF(macht%(koopt%-1) AND sticht%)=macht%(koopt%-1) PRINTTAB(0,20)A$'A$:VDU7:GOTO3232
 3240 PROCkoopt(speler%)
 3245 PROCprint_geld
 3250 PROCinfo_aandelen
 3255 PROCinformatie_ketens
 3260 PROCprint_stenen(speler%)
 3290 ENDPROC
 3291 ::::::::::::::::::::::::
 3500 DEFPROCprint_stenen(speler%)
 3502 tabx%=0:taby%=17
 3505 FORN%=1TO4
 3510 xpos%=(speelsteen%(N%,speler%)DIV15):IFxpos%=0 xpos%=15
 3515 ypos%=(speelsteen%(N%,speler%)MOD15):IFypos%=0 ypos%=15
 3520 COLOUR4:COLOUR134:PRINTTAB(tabx%,taby%)STR$N%;:COLOUR7:COLOUR128
 3521 nop%(0,N%)=xpos%:nop%(1,N%)=ypos%
 3525 PRINTCHR$(ypos%+&40)STR$xpos%" ";:tabx%=tabx%+4
 3526 NEXT
 3528 taby%=taby%+1:tabx%=0
 3530 FORN%=5TO8
 3540 xpos%=(speelsteen%(N%,speler%)DIV15):IFxpos%=0 xpos%=15
 3550 ypos%=(speelsteen%(N%,speler%)MOD15):IFypos%=0 ypos%=15
 3551 nop%(0,N%)=xpos%:nop%(1,N%)=ypos%
 3555 COLOUR4:COLOUR134:PRINTTAB(tabx%,taby%)STR$N%;:COLOUR7:COLOUR128
 3560 PRINTCHR$(ypos%+&40)STR$xpos%" ";
 3565 tabx%=tabx%+4
 3570 NEXT
 3571 COLOUR3:FORN%=1 TO 8:PRINTTAB(nop%(0,N%),nop%(1,N%))N%;:NEXT:COLOUR3
 3575 COLOUR3:COLOUR128
 3576 ENDPROC
 3577 ::::::::::::::::::::::
 3582 DEFPROCcomp(speler%)
 3583 PROCprint_geld:IFSPELER%>0 PROCprint_stenen(SPELER%)
 3585 IFsticht%>0 PROCstichten:PRINTTAB(0,19)"          ":IFgesticht%=TRUE aandelen%(kleur%,speler%)=aandelen%(kleur%,speler%)+1:GOTO3680
 3590 PROCbepaal_gunstigste_legsteen
 3600 IFleggen%=FALSE PRINTTAB(0,23)speler$(speler%)" kan niet":kanniet%=kanniet%+1:GOTO3720      
 3610 PRINTTAB(0,24)speler$(speler%)" legt "CHR$(ypos%+&40)STR$xpos%"   ";
 3680 plus%=1:doe%=TRUE
 3690 PROCsteen_leggen(xpos%,ypos%,kleur%)
 3695 PROCnieuwe_speelsteen_in_array(t%,speler%)
 3710 IFaanliggen%=TRUE PROCverander_na_fusie
 3715 IFverkopen%=TRUE AND W%>110 PROCverkopen
 3720 PROCinformatie_max
 3730 PROCinformatie_ketens
 3740 PROCkopen(speler%)
 3742 PROCprint_geld
 3743 PROCinformatie_ketens
 3745 PROCinfo_aandelen
 3746 doe%=FALSE
 3750 ENDPROC
 3751 ::::::::::::::::::::
 4000 DEFPROCspelen
 4005 verkopen%=FALSE:j%=FALSE:kanniet%=0
 4010 FORspeler%=1TO6
 4011 PROCwis:PRINTTAB(0,22)"Speler "speler%"  "speler$(speler%)"   "
 4013 IFkanniet%=6 GOTO4050
 4014 W%=W%+1
 4015 IFW%>=210 GOTO4050
 4016 FORM%=1TO8:IFFNmax(M%)>100 j%=FALSE:GOTO4050 ELSE NEXT
 4020 PROCsteen_bepalen(speler%)
 4030 NEXT
 4040 GOTO4005
 4041 :::::::::::::::::::::
 4045 DEFPROCscore
 4050 Y=OPENIN"SCORE"
 4060 FORN%=1TO30
 4070 INPUT#Y,array%(N%),array$(N%)
 4100 NEXT
 4105 CLOSE#Y
 4110 FORN%=31TO36
 4120 array%(N%)=vermogen%(N%-30)
 4125 array$(N%)=speler$(N%-30)
 4130 NEXT
 4135 IFj%=TRUE ENDPROC
 4140 PROCsortpointer(1,36)
 4141 CLOSE#0
 4145 CLS
 4146 PRINT"NR    SPELER    VERMOGEN"
 4150 Y=OPENOUT("SCORE")
 4160 FORN%=1TO24
 4170 PRINT#Y,array%(arraypo%(N%))
 4180 PRINT#Y,array$(arraypo%(N%))
 4181 PRINTN%;TAB(7)array$(arraypo%(N%));TAB(17)array%(arraypo%(N%))
 4190 NEXT
 4195 CLOSE#Y
 4199 *FX15,1
 4200 A$=INKEY$(6000)
 4210 RUN
 4250 ENDPROC
 4251 :::::::::::::::::::::::::
 4260 PROCsteen_leggen(xpos%,ypos%,10)
 4261 PROCcontroleer_vrij(xpos%,ypos%)
 4262 IFgesticht%=FALSE PROCsteen_leggen(xpos%,ypos%,-1)
 4265 IF(sticht% AND macht%(kl%-1)<>macht%(kl%-1)) gesticht%=FALSE:ENDPROC
 4270 kleur%=kl%
 4280 GOTO4680
 4281 :::::::::::::::::::::::::
 4500 DEFPROCstichten
 4502 PRINTTAB(0,19)"STICHTEN"
 4504 IFmens%=TRUE GOTO4260
 4505 t%=0:kleur%=0
 4506 plus%=0
 4510 REPEAT
 4515 t%=t%+1
 4520 PROCspeelsteen_uit_array_nemen(t%,speler%)
 4521 PROCsteen_leggen(xpos%,ypos%,10)
 4525 PROCcontroleer_vrij(xpos%,ypos%)
 4530 IFgesticht%=FALSE PROCsteen_leggen(xpos%,ypos%,-1)
 4535 UNTIL(gesticht%=TRUE OR t%=8)
 4540 IFgesticht%=FALSE ENDPROC
 4550 tel%=1:kleur%=0
 4570 FOR kleurs%=7 TO 0STEP-1
 4580 IF(sticht% AND macht%(kleurs%))=macht%(kleurs%) kleur%=kleur%+tel%*(kleurs%+1):tel%=tel%*10
 4590 NEXT
 4595 IFkleur%<9 GOTO4680
 4596 K%=kleur%:L%=K%
 4600 N%=LOGtel%:found%=FALSE
 4605 FORM%=0 TO N%
 4606 kleur%=L% DIV (10^(N%-M%)):L%=L%MOD(10^(N%-M%))
 4610 A1%=aandeelsort%(kleur%,1)MOD10:A2%=aandeelsort%(kleur%,2)MOD10
 4615 IFA1%=speler% OR A2%=speler% M%=10:found%=TRUE
 4620 NEXT
 4625 IFkleur%=0 kleur%=K% DIV (tel%/10)
 4630 IF found%=FALSE kleur%=VAL(MID$(STR$(K%),RND(N%),1))
 4680 sticht%=sticht%-macht%(kleur%-1)
 4685 aanliggen%=FALSE
 4686 PRINTTAB(0,24)speler$(speler%)" legt "CHR$(ypos%+&40)STR$xpos%"    ";
 4690 ENDPROC
 4691 ::::::::::::::::::::::::::::
 4700 DEFPROCsort(ST%,FIN%)
 4710 IFST%>=FIN% ENDPROC
 4720 LOCALF%,I%
 4730 REPEAT
 4740 F%=FALSE
 4750 FORI%=ST%TOFIN%-1
 4760 IFarray%(I%)<array%(I%+1) PROCswap
 4770 NEXT
 4780 FIN%=FIN%-1
 4790 UNTILNOTF%
 4795 ENDPROC
 4800 DEFPROCswap
 4810 LOCALtemp
 4820 temp=array%(I%)
 4830 array%(I%)=array%(I%+1)
 4840 array%(I%+1)=temp
 4850 F%=TRUE
 4860 ENDPROC
 4870 DEFPROCsortpointer(ST%,FIN%)
 4875 IFST%>=FIN% ENDPROC
 4876 FORI%=ST%TOFIN%:arraypo%(I%)=I%:NEXT
 4880 LOCALF%,I%
 4881 REPEAT
 4885 F%=FALSE
 4887 FORI%=ST%TOFIN%-1
 4890 IFarray%(arraypo%(I%))<array%(arraypo%(I%+1)) PROCswappo
 4895 NEXT
 4900 FIN%=FIN%-1
 4905 UNTILNOTF%
 4910 ENDPROC
 4920 DEFPROCswappo
 4925 LOCALtemp
 4930 temp=arraypo%(I%)
 4935 arraypo%(I%)=arraypo%(I%+1)
 4940 arraypo%(I%+1)=temp
 4945 F%=TRUE
 4950 ENDPROC
 5000 DEFPROCoverzicht
 5005 IFspeler%=SPELER% mens%=TRUE ELSE mens%=FALSE
 5010 CLS
 5020 PROCscore
 5031 PRINT"NR    SPELER    VERMOGEN"
 5032 FORN%=1TO30
 5033 PRINTN%;TAB(7)array$(N%)TAB(17)array%(N%)
 5034 NEXT
 5035 IFGET
 5040 CLS
 5050 PROCbord_uitprinten
 5055 PROCinformatie_ketens
 5060 PROCinformatie_max
 5065 PROCinfo_aandelen
 5070 PROCprint_geld
 5075 PROCprint_stenen(speler%)
 5080 PRINTTAB(0,23)"Speler "speler%"  "speler$(speler%)"     "
 5190 ENDPROC
 5200 DEFPROCbepaal_gunstigste_legsteen
 5210 t%=0:doe%=FALSE
 5220 REPEAT
 5230 t%=t%+1
 5240 PROCspeelsteen_uit_array_nemen(t%,speler%)
 5250 IFsteen%=-16 PROCwaarde_steen(speelsteen%,0,t%):GOTO5300
 5260 plus%=0:PROCsteen_leggen(xpos%,ypos%,10):PROCcontroleer_aanliggen(xpos%,ypos%):PROClege_steen_terug(xpos%,ypos%)
 5270 IF(kanniet%=TRUE AND aanliggen%=FALSE) PROCwaarde_steen(speelsteen%,0,t%):GOTO5300
 5280 PROCbepaal_waarde_steen(t%):PROCwaarde_steen(speelsteen%,waarde%,t%)
 5300 UNTILt%=8
 5310 FORN%=1TO8:array%(N%)=arraynop%(N%):NEXT:PROCsort(1,8)
 5320 IFINT(array%(1)/1000)=0 leggen%=FALSE:ENDPROC ELSE leggen%=TRUE
 5329 I%=1
 5330 t%=array%(I%)MOD1000
 5331 kleur%=array%(I%)MOD1000
 5332 IF(FNmax(kleur%)>15 AND array%(I%+1)>0 AND array%(I%)<9000) I%=I%+1:GOTO5330
 5333 IFkleur%=10 array%(I%)=0:I%=I%-1:GOTO5330
 5336 PROCspeelsteen_uit_array_nemen(t%,speler%)
 5340 doe%=TRUE
 5341 PROCsteen_leggen(xpos%,ypos%,10)
 5342 PROCcontroleer_aanliggen(xpos%,ypos%)
 5345 PROClege_steen_terug(xpos%,ypos%)
 5350 ENDPROC
 5351 :::::::::::::::::::::::::
 5500 DEFPROCwaarde_steen(speelsteen%,waarde%,nummer%)
 5510 arraynop%(nummer%)=1000*waarde%+t%
 5520 ENDPROC
 5521 :::::::::::::::::::::::::::
 5550 DEFPROCbepaal_waarde_steen(t%)
 5555 IFkanniet%=TRUE waarde%=0:ENDPROC
 5560 IFaanliggen%=TRUE PROCbekijk_fusie
 5570 IFaanliggen%=FALSE waarde%=2
 5990 ENDPROC
 5991 :::::::::::::::::::::::::::
 6000 DEFPROCwis
 6010 PRINTTAB(0,19)A$'A$'A$'A$'A$'A$;
 6020 ENDPROC
 6021 ::::::::::::::::::::::::::
 7000 DEFPROCbekijk_fusie
 7005 waardemax%=0
 7010 FORN%=1TOS%
 7036 waarde%=4
 7040 IF((aandeelsort%(kleinere(N%),1)MOD10=speler% OR aandeelsort%(kleinere(N%),2)MOD10=speler%) AND kleinere(N%)<>MAX%) waarde%=10
 7041 IF((aandeelsort%(kleinere(N%),1)MOD10=speler% OR aandeelsort%(kleinere(N%),2)MOD10=speler%) AND kleinere(N%)=MAX%) waarde%=5
 7045 FORM%=3TO6
 7046 IF(aandeelsort%(kleinere(N%),M%)MOD10=speler%) waarde%=2:M%=7:GOTO7047
 7047 NEXT
 7065 IFwaarde%>waardemax% waardemax%=waarde%
 7070 NEXT
 7075 waarde%=waardemax%
 7080 ENDPROC
 7081 ::::::::::::::::::::::::
 9040 DEFPROCsort_aandelen
 9050 FORM%=1TO8
 9060 FORN%=1TO6
 9100 array%(N%)=10*aandelen%(M%,N%)+N%
 9110 NEXT
 9120 PROCsortpointer(1,6)
 9130 FORO%=1TO6
 9140 aandeelsort%(M%,O%)=array%(arraypo%(O%))
 9150 NEXT
 9155 NEXT
 9160 ENDPROC
 9161 :::::::::::::::::::::::
 9180 DEFPROCkopen(speler%)
 9185 PROCsort_aandelen
 9190 tel%=1:koop%=0
 9195 FORO%=4TO0STEP-1
 9200 FORN%=1TO3
 9210 FORM%=1TO8
 9215 IF(sticht% AND macht%(M%-1))=macht%(M%-1) GOTO9230
 9219 A1%=aandeelsort%(M%,N%)DIV10:A2%=aandeelsort%(M%,N%+1)DIV10:A3%=aandeelsort%(M%,N%)MOD10:A4%=aandeelsort%(M%,N%+2)DIV10
 9220 IF(A3%=speler% AND A2%+O%>A1%) koop%=koop%+tel%*M%:tel%=tel%*10
 9225 IF(A3%=speler% AND A2%=A4%) koop%=koop%+tel%*M%:tel%=tel%*10
 9230 IFtel%>1000 M%=8:N%=3
 9240 NEXT
 9250 NEXT
 9260 IFkoop%>0 O%=0
 9270 NEXT
 9275 IFkoop%=0 koop%=RND(8):tel%=1:IFmacht%(koop%-1) AND sticht%=macht%(koop%-1)koop%=0:GOTO9275
 9279 R%=LOGtel%
 9280 IFtel%<11 koopt%=koop%:GOTO9330
 9290 N%=tel%:REM N%=10^R%
 9310 koopt%=koop%MODN%DIV(N%DIV10)
 9330 A1%=aandeelsort%(koopt%,1)DIV10:A2%=aandeelsort%(koopt%,2)DIV10:A3%=aandeelsort%(koopt%,1)MOD10:A4%=aandeelsort%(koopt%,2)MOD10:A5%=aandeelsort%(koopt%,3)DIV10
 9331 IF(A3%=speler% AND A1%-A2%>4) R%=R%-1:GOTO9335ELSEGOTO9345
 9332 IF(A4%=speler% AND A2%-A5%>4) R%=R%-1:GOTO9335ELSEGOTO9340
 9335 IFR%=0 koop%=0:GOTO9275 ELSE IF R%>0 GOTO9290
 9345 PROCkoopt(speler%)
 9350 ENDPROC
 9351 ::::::::::::::::::::::
 9390 DEFPROCkoopt(speler%)
 9400 N%=3:nN%=FALSE
 9401 IF mens% N%=koopaantal%
 9410 REPEAT
 9415 IFbezit%(speler%)>prijs_keten%(koopt%)*N%:koopaantal%=N%:nN%=TRUE ELSE N%=N%-1
 9420 UNTILnN%=TRUE
 9490 aandelen%(koopt%,speler%)=aandelen%(koopt%,speler%)+koopaantal%
 9500 bezit%(speler%)=bezit%(speler%)-prijs_keten%(koopt%)*koopaantal%
 9506 IFkoopaantal%=0 PRINTTAB(0,23)speler$(speler%)" koopt niets     ":GOTO9580
 9510 PRINTTAB(0,23)speler$(speler%)" koopt "koopaantal%"*"CHR$(&40+koopt%)
 9580 PROCsort_aandelen
 9590 ENDPROC
 9591 :::::::::::::::::::::::
20000 DATA900,900,800,700,700,600,500,400:REM DATA PRIJS KETEN    
20001 :::::::::::::::::::::::
25000 DEFFNketen(x%,y%)
25010 sop%=?(&A00+15*x%+y%)
25011 IFsop%=99 THEN sop%=-1
25012 =sop%
25013 :::::::::::::::::::::::
25020 DEFPROCpoke_keten(x%,y%,kleur%)
25025 IFkleur%=-1 THEN kleur%=99
25030 ?(&A00+15*x%+y%)=kleur%
25040 ENDPROC
25041 ::::::::::::::::::::::
25050 DEFFNsteen(x%,y%)
25060 =?(&900+15*(y%-1)+x%-1)
25061 :::::::::::::::::::::::
25070 DEFPROCpoke_steen(x%,y%,nummer%)
25080 ?(&900+15*(y%-1)+x%-1)=nummer%
25090 ENDPROC
25091 :::::::::::::::::::::::
26000 DEFFNmax(nn%)
26005 IF nn%=0 =0
26010 =?(&B30+nn%)
26011 ::::::::::::::::::::::::
26020 DEFPROCpoke_max(nn%,plus%)
26030 ?(&B30+nn%)=?(&B30+nn%)+plus%
26040 ENDPROC
26041 ::::::::::::::::::::::::::
29000 DATA7,128
29001 DATA7,130  
29002 DATA4,131     
29003 DATA7,132     
29004 DATA7,133 
29005 DATA7,134     
29006 DATA4,135    
29007 DATA7,129
29008 :::::::::::::::::::::::::
30000 FORN%=1TO7
30010 FORM%=128TO135
30020 COLOURN%:COLOURM%
30030 PRINT"ABCDEFGH"
30040 NEXT
30050 NEXT