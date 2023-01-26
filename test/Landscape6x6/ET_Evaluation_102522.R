#Processes PnET-site files for PET, AET, Ts evaluation.
#Gordon Reese, 2022

# remove previously loaded objects
rm(list=ls(all=TRUE))
options(scipen=999)

FolderBase <- "C:/BRM/LANDIS_II/GitCode/aruzicka555/Extension-PnET-Succession/test/Landscape6x6/output/"
FolderKids <- c("Site1-EvapDepth100_Leak0.01_Runoff100","Site2-EvapDepth100_Leak0.01_Runoff75","Site3-EvapDepth100_Leak0.01_Runoff50",
                "Site4-EvapDepth100_Leak0.1_Runoff100","Site5-EvapDepth100_Leak0.1_Runoff75","Site6-EvapDepth100_Leak0.1_Runoff50",
                "Site7-EvapDepth100_Leak0.5_Runoff100","Site8-EvapDepth100_Leak0.5_Runoff75","Site9-EvapDepth100_Leak0.5_Runoff50",
                "Site10-EvapDepth500_Leak0.01_Runoff100","Site11-EvapDepth500_Leak0.01_Runoff75","Site12-EvapDepth500_Leak0.01_Runoff50",
                "Site13-EvapDepth500_Leak0.1_Runoff100","Site14-EvapDepth500_Leak0.1_Runoff75","Site15-EvapDepth500_Leak0.1_Runoff50",
                "Site16-EvapDepth500_Leak0.5_Runoff100","Site17-EvapDepth500_Leak0.5_Runoff75","Site18-EvapDepth500_Leak0.5_Runoff50",
                "Site19-EvapDepth1000_Leak0.01_Runoff100","Site20-EvapDepth1000_Leak0.01_Runoff75","Site21-EvapDepth1000_Leak0.01_Runoff50",
                "Site22-EvapDepth1000_Leak0.1_Runoff100","Site23-EvapDepth1000_Leak0.1_Runoff75","Site24-EvapDepth1000_Leak0.1_Runoff50",
                "Site25-EvapDepth1000_Leak0.5_Runoff100","Site26-EvapDepth1000_Leak0.5_Runoff75","Site27-EvapDepth1000_Leak0.5_Runoff50",
                "Site28-EvapDepth100_Leak1_Runoff0","Site29-EvapDepth500_Leak1_Runoff0","Site30-EvapDepth1000_Leak1_Runoff0",
                "Site31-EvapDepth100_Leak1_Runoff0","Site32-EvapDepth500_Leak1_Runoff0","Site33-EvapDepth1000_Leak1_Runoff0",
                "Site34-EvapDepth100_Leak1_Runoff0","Site35-EvapDepth500_Leak1_Runoff0","Site36-EvapDepth1000_Leak1_Runoff0")

#Start creation of pdf
setwd("C:/BRM/LANDIS_II/GitCode/aruzicka555/Extension-PnET-Succession/test/Landscape6x6/")
pdf("Landscape6x6_PnETv5.0rc23_noEstbl.pdf")

#For set of pages of water balance attributes
for(i in 1:36){
  Folder <- paste(FolderBase, FolderKids[i], sep="")
  setwd(Folder)
  
  SiteFile <- read.csv(file="Site.csv",header=TRUE)
  #Get Precip, RunOff, Leakage, PET, PE, Evap, PTs, Ts, Intercept, AvailWater; AND calculate AET
  # For 2nd graph: LAI, Water, PressureHead
  # For 3rd graph: Wood
  SiteCols <- SiteFile[,c(15, 18, 19, 20, 21, 22, 23, 24, 25, 27, 28, 29, 30, 31,32, 37)]
  SiteCols$AET <- SiteCols$Evaporation.mm. + SiteCols$Transpiration.mm. + SiteCols$Interception.mm.
  SiteCols$Year <- rep(1:409, each=12)
  rm(SiteFile)
  
  #Summarize the variables to be plotted
  PrecipSum <- aggregate(SiteCols$Precip.mm.mo., by=list(SiteCols$Year), FUN="sum")
  RunOffSum <- aggregate(SiteCols$RunOff.mm.mo., by=list(SiteCols$Year), FUN="sum")
  LeakageSum <- aggregate(SiteCols$Leakage.mm., by=list(SiteCols$Year), FUN="sum")
  PETSum <- aggregate(SiteCols$Potential.Evapotranspiration.mm., by=list(SiteCols$Year), FUN="sum")
  PESum <- aggregate(SiteCols$Potential.Evaporation.mm., by=list(SiteCols$Year), FUN="sum")
  EvapSum <- aggregate(SiteCols$Evaporation.mm., by=list(SiteCols$Year), FUN="sum")
  PTsSum <- aggregate(SiteCols$PotentialTranspiration.mm., by=list(SiteCols$Year), FUN="sum")
  TsSum <- aggregate(SiteCols$Transpiration.mm., by=list(SiteCols$Year), FUN="sum")
  InterceptSum <- aggregate(SiteCols$Interception.mm., by=list(SiteCols$Year), FUN="sum")
  AvailWaterAvg <- aggregate(SiteCols$Available.Water..mm., by=list(SiteCols$Year), FUN="mean")
  AETSum <- aggregate(SiteCols$AET, by=list(SiteCols$Year), FUN="sum")
  SurfWaterAvg <- aggregate(SiteCols$Surface.Water..mm., by=list(SiteCols$Year), FUN="mean")
  
  plot(1, type ="n", xlab="Year", ylab="mm", xlim=c(0,410), ylim=c(-5,1100))

  if(i<28){
    title(paste(FolderKids[i]), "Mucky peat: black ash, longevity=150", sep=" ")
  } else if(i<31){
    title(paste(FolderKids[i]), "Clay: sugar maple, longevity=300", sep=" ")
  } else if(i<34){
    title(paste(FolderKids[i]), "Coarse sand: sugar maple, longevity=300", sep=" ")
  } else{
    title(paste(FolderKids[i]), "Silt loam: sugar maple, longevity=300", sep=" ")
  }

  lines(PrecipSum, col="blue")
  lines(RunOffSum, col="darkorange")
  lines(LeakageSum, col="darkgrey")
  lines(PETSum, col="red")
  lines(PESum, col="cyan")
  lines(EvapSum, col="chartreuse4")
  lines(PTsSum, col="darkorchid2")
  lines(TsSum, col="brown")
  lines(InterceptSum, col="yellow")
  lines(AvailWaterAvg, col="black")
  lines(AETSum, col="deepskyblue1")
  lines(SurfWaterAvg, col = "pink")
  
  #op <- par(cex = 0.7)
  legend('topright', c("Prec(mm/yr)","RunOff(mm/yr)","Leakage(mm)","PET(mm)","PE(mm)","Evap(mm)","PT(mm)"
                       ,"T(mm)","Intercep.(mm)","Avail H20(mm)","AET(mm)","SurfWtr(mm)"), bg="gray60",
         text.col = c("blue","darkorange","darkgrey","red","cyan","chartreuse4","darkorchid2","brown",
                      "yellow","black","deepskyblue1","pink"))
} #end for 1:36; water balance loop


#For set of pages with LAI, Pressure head, & Water
for(i in 1:36){
  Folder <- paste(FolderBase, FolderKids[i], sep="")
  setwd(Folder)
  
  SiteFile <- read.csv(file="Site.csv",header=TRUE)
  #Get Precip, RunOff, Leakage, PET, PE, Evap, PTs, Ts, Intercept, AvailWater; AND calculate AET
  # For 2nd graph: LAI, Water, PressureHead
  # For 3rd graph: Wood
  SiteCols <- SiteFile[,c(15, 18, 19, 20, 21, 22, 23, 24, 25, 27, 28, 29, 30, 31,32, 37)]
  SiteCols$AET <- SiteCols$Evaporation.mm. + SiteCols$Transpiration.mm. + SiteCols$Interception.mm.
  SiteCols$Year <- rep(1:409, each=12)
  rm(SiteFile)
  
  #Summarize the variables being plotted
  LAIMax <- aggregate(SiteCols$LAI.m2., by=list(SiteCols$Year), FUN="max")
  WaterAvg <- aggregate(SiteCols$Water.m.m., by=list(SiteCols$Year), FUN="mean")
  PHeadAvg <- aggregate(SiteCols$PressureHead.mm., by=list(SiteCols$Year), FUN="mean")
  
  plot(1, type ="n", xlab="Year", ylab="mm", xlim=c(0,410), ylim=c(0,20))
  
  if(i<28){
    title(paste(FolderKids[i]), "Mucky peat: black ash, longevity=150", sep=" ")
  } else if(i<31){
    title(paste(FolderKids[i]), "Clay: sugar maple, longevity=300", sep=" ")
  } else if(i<34){
    title(paste(FolderKids[i]), "Coarse sand: sugar maple, longevity=300", sep=" ")
  } else{
    title(paste(FolderKids[i]), "Silt loam: sugar maple, longevity=300", sep=" ")
  }
  
  lines(LAIMax, col="blue")
  lines(WaterAvg, col="darkorange")
  lines(PHeadAvg, col="darkgrey")

  legend('topright', c("max LAI(m2)","ave Water(m/m)","ave Pr-head(mm)"), bg="gray60",
         text.col = c("blue","darkorange","darkgrey"))
} #end for 1:36; LAI, PressureHead loop


#For set of pages with Wood sum
for(i in 1:36){
  Folder <- paste(FolderBase, FolderKids[i], sep="")
  setwd(Folder)
  
  SiteFile <- read.csv(file="Site.csv",header=TRUE)
  #Get Precip, RunOff, Leakage, PET, PE, Evap, PTs, Ts, Intercept, AvailWater; AND calculate AET
  # For 2nd graph: LAI, Water, PressureHead
  # For 3rd graph: Wood
  SiteCols <- SiteFile[,c(15, 18, 19, 20, 21, 22, 23, 24, 25, 27, 28, 29, 30, 31,32, 37)]
  SiteCols$AET <- SiteCols$Evaporation.mm. + SiteCols$Transpiration.mm. + SiteCols$Interception.mm.
  SiteCols$Year <- rep(1:409, each=12)
  rm(SiteFile)
  
  #Summarize the variables being plotted
  WoodAvg <- aggregate(SiteCols$Wood.gDW., by=list(SiteCols$Year), FUN="mean")

  plot(1, type ="n", xlab="Year", ylab="Total (gDW)", xlim=c(0,410), ylim=c(0,25000))
  
  if(i<28){
    title(paste(FolderKids[i]), "Mucky peat: black ash, longevity=150", sep=" ")
  } else if(i<31){
    title(paste(FolderKids[i]), "Clay: sugar maple, longevity=300", sep=" ")
  } else if(i<34){
    title(paste(FolderKids[i]), "Coarse sand: sugar maple, longevity=300", sep=" ")
  } else{
    title(paste(FolderKids[i]), "Silt loam: sugar maple, longevity=300", sep=" ")
  }
  
  lines(WoodAvg, col="blue")

  legend('topright', c("ave Wood"), bg="gray60", text.col = c("blue"))
} #end for 1:36; Wood biomass loop


#For set of pages with fWater
for(i in 1:36){
  Folder <- paste(FolderBase, FolderKids[i], sep="")
  setwd(Folder)
  
  if(i<28){
    CohortFile <- read.csv(file="Cohort_BlackAsh_1791.csv",header=TRUE)
                                                                                    #Not sure next will be a consistent number of years
    NumYears <- nrow(CohortFile) / 12
    CohortFile$Year2 <- rep(1:NumYears, each=12)
  } else{
    CohortFile <- read.csv(file="Cohort_SugarMaple_1791.csv",header=TRUE)
                                                                                    #Not sure next will be a consistent number of years, so
    NumYears <- nrow(CohortFile) / 12
    CohortFile$Year2 <- rep(1:NumYears, each=12)
                                                                                    #Due to previous, slice to consistent number of years (279)
    CohortFile <- CohortFile[1:3348,]
  }
  
  CohortCols <- CohortFile[,c(24,26,56)]
  rm(CohortFile)
  
  #Summarize the variables being plotted
  fWaterAvg <- aggregate(CohortCols$fWater..., by=list(CohortCols$Year2), FUN="mean")
  CohPHeadAvg <- aggregate(CohortCols$PressureHead.mm., by=list(CohortCols$Year2), FUN="mean")
  
  plot(1, type ="n", xlab="Year", ylab="Cohort file", xlim=c(0,300), ylim=c(0,50))
  
  if(i<28){
    title(paste(FolderKids[i]), "Mucky peat: black ash, longevity=150", sep=" ")
  } else if(i<31){
    title(paste(FolderKids[i]), "Clay: sugar maple, longevity=300", sep=" ")
  } else if(i<34){
    title(paste(FolderKids[i]), "Coarse sand: sugar maple, longevity=300", sep=" ")
  } else{
    title(paste(FolderKids[i]), "Silt loam: sugar maple, longevity=300", sep=" ")
  }
  
  lines(fWaterAvg, col="blue")
  lines(CohPHeadAvg, col="darkorange")
  
  legend('topright', c("ave fWater","ave PHead"), bg="gray60", text.col = c("blue","darkorange"))
} #end for 1:36; fWater loop

dev.off()
