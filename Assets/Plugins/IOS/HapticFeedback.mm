#import <UIKit/UIKit.h>

extern "C" {
    void _TriggerLightHaptic() {
        if (@available(iOS 10.0, *)) {
            UIImpactFeedbackGenerator *gen = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleLight];
            [gen impactOccurred];
        }
    }
    
    void _TriggerMediumHaptic() {
        if (@available(iOS 10.0, *)) {
            UIImpactFeedbackGenerator *gen = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleMedium];
            [gen impactOccurred];
        }
    }
    
    void _TriggerHeavyHaptic() {
        if (@available(iOS 10.0, *)) {
            UIImpactFeedbackGenerator *gen = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleHeavy];
            [gen impactOccurred];
        }
    }
}