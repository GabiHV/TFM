#include "apriltag.h"

#include "tag36h11.h"
#include "tag16h5.h"
#include "tag25h9.h"
#include "tag36h10.h"
#include "tagCircle21h7.h"
#include "tagCircle49h12.h"
#include "tagCustom48h12.h"
#include "tagStandard41h12.h"
#include "tagStandard52h13.h"
#include "apriltag_pose.h"
#include <stdint.h>
#include <stdio.h>

#if _WIN32
#define EXPORT __declspec(dllexport)
#else
#define EXPORT
#endif

#if __cplusplus
extern "C" {
#endif

typedef struct
{
    int id;
    double cx;
    double cy;
} detection_info_t;


static apriltag_family_t* tf = NULL;
static apriltag_detector_t* td = NULL;
static char* family_name = "tag36h11";
zarray_t* detections;

void apriltag_init(char* apriltag_family_name) {
    if(family_name == "tag36h11"){
        tf = tag36h11_create();
    } else if(family_name == "tag16h5"){
        tf = tag16h5_create();
    } else if(family_name == "tag25h9"){
        tf = tag25h9_create();
    } else if(family_name == "tag36h10"){
        tf = tag36h10_create();
    } else if(family_name == "tagCircle21h7"){
        tf = tagCircle21h7_create();
    } else if(family_name == "tagCircle49h12"){
        tf = tagCircle49h12_create();
    } else if(family_name == "tagCustom48h12"){
        tf = tagCustom48h12_create();
    } else if(family_name == "tagStandard41h12"){
        tf = tagStandard41h12_create();
    } else if(family_name == "tagStandard52h13"){
        tf = tagStandard52h13_create();
    } else {
        return;
    }
    family_name = apriltag_family_name;
    td = apriltag_detector_create();

    apriltag_detector_add_family(td, tf);
}

void apriltag_destroy() {    
    if(tf == NULL && td == NULL) return;
    if(tf == NULL) return;
    if(td == NULL) return;

    if(family_name == "tag36h11"){
        tag36h11_destroy(tf);
    } else if(family_name == "tag16h5"){
        tag16h5_destroy(tf);
    } else if(family_name == "tag25h9"){
        tag25h9_destroy(tf);
    } else if(family_name == "tag36h10"){
        tag36h10_destroy(tf);
    } else if(family_name == "tagCircle21h7"){
        tagCircle21h7_destroy(tf);
    } else if(family_name == "tagCircle49h12"){
        tagCircle49h12_destroy(tf);
    } else if(family_name == "tagCustom48h12"){
        tagCustom48h12_destroy(tf);
    } else if(family_name == "tagStandard41h12"){
        tagStandard41h12_destroy(tf);
    } else if(family_name == "tagStandard52h13"){
        tagStandard52h13_destroy(tf);
    } else {
        return;
    }

    apriltag_detector_destroy(td);
    apriltag_detections_destroy(detections);
}

int apriltag_num_tag(uint8_t* grayImage, int width, int height){
    if(td == NULL || tf == NULL) return -1;

    image_u8_t im = 
    { 
        .width = width, 
        .height = height, 
        .stride = width, 
        .buf = grayImage 
    };
    detections = apriltag_detector_detect(td, &im);
    int array_size = zarray_size(detections);

    return array_size;
}

int apriltag_detect(detection_info_t* outArray) {
    if(td == NULL || tf == NULL) return -1;

    int array_size = zarray_size(detections);
        
    for(int i = 0; i < array_size; i++)
    {
        apriltag_detection_t* det;
        zarray_get(detections, i, &det); 

        outArray[i].id = det->id;
        outArray[i].cx = det->c[0];
        outArray[i].cy = det->c[1];
    }

    return array_size;
}

#if __cplusplus
}
#endif
