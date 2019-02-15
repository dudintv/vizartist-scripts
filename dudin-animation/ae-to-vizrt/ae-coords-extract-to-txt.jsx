// Script created by mlk: mlkdesign@gmail.com Jan 2007 
// Modified by Dmitry Dudin dudin.tv@gmail.com Apr 2015 for AE CC
// 
// The script will write to a text file values of the layer position, rotation, orientation and scale for every frame comprised in
// a selection of keyframes, or for the whole comp duration if no keyframes are selected

function TimeToFrameNum(myTime){
	return Math.floor(myTime) * app.project.activeItem.frameRate + (myTime - Math.floor(myTime)) / (1/app.project.activeItem.frameRate);
}
function FramesToTime(myTime){
	return myTime/app.project.activeItem.frameRate;
}
function AddZero(val){
	if(val<10){
		val = "0"+val;
	}
	return val;
}
function TimeToTimeCode(myTime){
	var framesN = myTime * app.project.activeItem.frameRate;
	fr = addZero(Math.round((myTime - Math.floor(myTime))/(1/app.project.activeItem.frameRate)));
	ho = addZero(Math.floor(myTime/3600));
	mi = addZero(Math.floor(myTime/60)-ho*60);
	se = addZero(Math.floor(myTime)-mi*60-ho*3600);
	return ho+":"+mi+":"+se+":"+fr;
}
function GetZoneKeys(curLayer, nameProperty){
    selKeys = curLayer.property(nameProperty).selectedKeys;
    if(selKeys.length != 0){
        startTime = curLayer.property(nameProperty).keyTime(selKeys[0]);
        endTime  = curLayer.property(nameProperty).keyTime(selKeys[selKeys.length-1]);
    }else{
        //startTime = app.project.activeItem.workAreaStart;
        //endTime  = app.project.activeItem.workAreaStart + app.project.activeItem.workAreaDuration;
        startTime = -1;
        endTime = -1;
    }
    return [  TimeToFrameNum(startTime),  TimeToFrameNum(endTime)  ];
}

function GetTrasformDataFromOneLayerAtTime(format, curLayer,curTime){
    out = format;
    out = out.replace('*i',curTime);
    out = out.replace('*x',curLayer.property("position").valueAtTime(curTime,true)[0]);
    out = out.replace('*y',curLayer.property("position").valueAtTime(curTime,true)[1]);
    out = out.replace('*z',curLayer.property("position").valueAtTime(curTime,true)[2]);
    out = out.replace('*rx',curLayer.property("X Rotation").valueAtTime(curTime,true));
    out = out.replace('*ry',curLayer.property("Y Rotation").valueAtTime(curTime,true));
    out = out.replace('*rz',curLayer.property("Z Rotation").valueAtTime(curTime,true));
    out = out.replace('*ox',curLayer.property("Orientation").valueAtTime(curTime,true)[0]);
    out = out.replace('*oy',curLayer.property("Orientation").valueAtTime(curTime,true)[1]);
    out = out.replace('*oz',curLayer.property("Orientation").valueAtTime(curTime,true)[2]);
    out = out.replace('*sx',curLayer.property("Scale").valueAtTime(curTime,true)[0]);
    out = out.replace('*sy',curLayer.property("Scale").valueAtTime(curTime,true)[1]);
    out = out.replace('*sz',curLayer.property("Scale").valueAtTime(curTime,true)[2]);
    return out;
}

function GetTransformDataFromOneLayer(format, curLayer){
    var out = "";
    var output = "";
    var empty = "-";
    
    //  zone of selected keys animation
    var zonePosition      = GetZoneKeys(curLayer, "Position");
    var zoneXRotation   = GetZoneKeys(curLayer, "X Rotation");
    var zoneYRotation   = GetZoneKeys(curLayer, "Y Rotation");
    var zoneZRotation   = GetZoneKeys(curLayer, "Z Rotation");
    var zoneOrientation = GetZoneKeys(curLayer, "Orientation");
    var zoneScale         = GetZoneKeys(curLayer, "Scale");
    
    //start and base parameters of trasformation
    output = output + curLayer.name + " | " + GetTrasformDataFromOneLayerAtTime(format, curLayer,0) + "\n"
    startLoop = TimeToFrameNum(  app.project.activeItem.workAreaStart  );
    endLoop  = TimeToFrameNum(  app.project.activeItem.workAreaStart + app.project.activeItem.workAreaDuration  );
    
    for(i=startLoop; i<=endLoop; i++){
        var curTime = FramesToTime(i);
        out = format;
        out = out.replace('*i',i-startLoop);
        
        if(i>=zonePosition[0] && i<=zonePosition[1] ){
            out = out.replace('*x',curLayer.property("position").valueAtTime(curTime,true)[0]);
            out = out.replace('*y',curLayer.property("position").valueAtTime(curTime,true)[1]);
            out = out.replace('*z',curLayer.property("position").valueAtTime(curTime,true)[2]);
        }else{
            out = out.replace('*x',empty);
            out = out.replace('*y',empty);
            out = out.replace('*z',empty);
        }
        
        if(i>=zoneXRotation[0] && i<=zoneXRotation[1] ){
            out = out.replace('*rx',curLayer.property("X Rotation").valueAtTime(curTime,true));
        }else{
            out = out.replace('*rx',empty)
        }
        if(i>=zoneYRotation[0] && i<=zoneYRotation[1] ){
            out = out.replace('*ry',curLayer.property("Y Rotation").valueAtTime(curTime,true));
        }else{
            out = out.replace('*ry',empty)
        }
        if(i>=zoneZRotation[0] && i<=zoneZRotation[1] ) {
            out = out.replace('*rz',curLayer.property("Z Rotation").valueAtTime(curTime,true));
        }else{
            out = out.replace('*rz',empty)
        }       
        
        if(i>=zoneOrientation[0] && i<=zoneOrientation[1] ){
            out = out.replace('*ox',curLayer.property("Orientation").valueAtTime(curTime,true)[0]);
            out = out.replace('*oy',curLayer.property("Orientation").valueAtTime(curTime,true)[1]);
            out = out.replace('*oz',curLayer.property("Orientation").valueAtTime(curTime,true)[2]);
        }else{
            out = out.replace('*ox',empty);
            out = out.replace('*oy',empty);
            out = out.replace('*oz',empty);
        }
        
        if(i>=zoneScale[0] && i<=zoneScale[1] ){
            out = out.replace('*sx',curLayer.property("Scale").valueAtTime(curTime,true)[0]);
            out = out.replace('*sy',curLayer.property("Scale").valueAtTime(curTime,true)[1]);
            out = out.replace('*sz',curLayer.property("Scale").valueAtTime(curTime,true)[2]);
        }else{
            out = out.replace('*sx',empty);
            out = out.replace('*sy',empty);
            out = out.replace('*sz',empty);
        }
        
        //out = out.replace('*f',i);timeToTimeCode
        //out = out.replace('*t',timeToTimeCode(curTime));
        
        output = output + out + "\n";
    }
    return output + "\n";
    
}

//-----------------------------------------------------------------------------------------------------------------------------------------------------------

var myDisp = app.project.timecodeDisplayType;
app.project.timecodeDisplayType = TimeDisplayType.TIMECODE;
var pText = "Choose an output format using:\n*f (framenumber),*i (index, starts at 0) *t (SMTPE timecode), and *x,*y,*z, *rx,*ry,*rz, *ox,*oy,*oz, *sx,*sy,*sz and any other character";

if(app.project.activeItem != "null" && app.project.activeItem != null && app.project.activeItem != 0){
	if(app.project.activeItem.selectedLayers.length != 0){
		if(app.preferences.getPrefAsLong("Main Pref Section","Pref_SCRIPTING_FILE_NETWORK_SECURITY")){
			var myTextFile = File.saveDialog("Select a location to save your .txt file", "Text: *.txt");
			if(myTextFile == null){
				alert("You must choose a place to save the file");
			}else{
                //  if all is OK :) let do work...
                
                
                
                var formatString = prompt(pText,"*i: *x *y *z *rx *ry *rz *ox *oy *oz *sx *sy *sz");
                myTextFile.open("w","TEXT","????");
                //   ake all SELECTED layers
                var countSelectLayer = app.project.activeItem.selectedLayers.length;
                for(var numLayer=0; numLayer<countSelectLayer; numLayer++){
                    myTextFile.write(  GetTransformDataFromOneLayer(formatString, app.project.activeItem.selectedLayers[numLayer])  );
                    clearOutput();
                    write(   Math.round(  numLayer/(countSelectLayer)*100,0  )+"% done..."  );
                }
                
                //  good finish
                clearOutput();
                writeLn(endLoop-startLoop+" trasnforamation data saved to file.");
                writeLn("script written by mlk & Dmitry Dudin =)");
                myTextFile.close();
                
                
                
			}
		} else {
			alert ("This script requires the scripting security preference to be set.\n" +
			"Go to the \"General\" panel of your application preferences,\n" +
			"and make sure that \"Allow Scripts to Write Files and Access Network\" is checked.");
		}
	}else{
		alert("Select a layer with 'position' keyframes");
	}
}else{
	alert("Select a composition and a layer with 'position' keyframes");
}
app.project.timecodeDisplayType = myDisp;