#import <AudioToolbox/AudioToolbox.h>
#import <CoreBluetooth/CoreBluetooth.h>
#import <Foundation/Foundation.h>
#import <MediaPlayer/MediaPlayer.h>
#import "UnityAppController.h"
#import <mach/mach.h>
#import <mach/mach_host.h>
#include <assert.h>
#include <stdbool.h>
#include <sys/types.h>
#include <unistd.h>
#include <sys/sysctl.h>


@interface NativeApplicationController : UnityAppController {}
@end

@implementation NativeApplicationController

- (void)applicationDidReceiveMemoryWarning:(UIApplication*)application
{
    UnitySendMessage("NativeBridge", "ReceivedMemoryWarning", "");
    
    [super applicationDidReceiveMemoryWarning:application];
}

@end

IMPL_APP_CONTROLLER_SUBCLASS(NativeApplicationController)

/**
 * @class BluetoothInfo
 * @authors Kevin O'Sullivan
 * @brief Provides bluetooth status.
 */
@interface BluetoothInfo : NSObject <CBCentralManagerDelegate>

@property (nonatomic, strong) CBCentralManager *centralManager;         //!< The CBCentralManager instance.
@property (nonatomic, strong) NSString *bluetoothCallbackObjectName;    //!< The C# callback object's name.

/*!
 * Used to get the shared instance of BluetoothInfo.
 @return BluetoothInfo (id)
 */
+ (id)sharedInstance;

/*!
 * Initiallizes the CBCentralManager reference.
 @return void
 */
- (void)initCentralManager;

@end

@implementation BluetoothInfo

static BluetoothInfo *bluetoothInfoInstance = nil;

+ (id)sharedInstance
{
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        bluetoothInfoInstance = [[BluetoothInfo alloc] init];
        [bluetoothInfoInstance initCentralManager];
    });
    
    return bluetoothInfoInstance;
}

- (void)initCentralManager
{
    _centralManager = [[CBCentralManager alloc] initWithDelegate:self
                                                           queue:nil];
}
- (void)updateBluetoothState:(NSString *)aCallbackName
{
    _bluetoothCallbackObjectName = aCallbackName;
    
    [self bluetoothState:_centralManager];
}

- (void)bluetoothState:(CBCentralManager *)central
{
    switch (central.state)
    {
        case CBCentralManagerStatePoweredOff:
            UnitySendMessage([_bluetoothCallbackObjectName UTF8String], "OnBluetoothState", "OFF");
            break;
            
        case CBCentralManagerStatePoweredOn:
            UnitySendMessage([_bluetoothCallbackObjectName UTF8String], "OnBluetoothState", "ON");
            break;
            
        case CBCentralManagerStateResetting:
            UnitySendMessage([_bluetoothCallbackObjectName UTF8String], "OnBluetoothState", "RESETTING");
            break;
            
        case CBCentralManagerStateUnsupported:
            UnitySendMessage([_bluetoothCallbackObjectName UTF8String], "OnBluetoothState", "UNSUPPORTED");
            break;
            
        case CBCentralManagerStateUnauthorized:
            UnitySendMessage([_bluetoothCallbackObjectName UTF8String], "OnBluetoothState", "UNAUTHORIZED");
            break;
            
        case CBCentralManagerStateUnknown:
            UnitySendMessage([_bluetoothCallbackObjectName UTF8String], "OnBluetoothState", "UNKNOWN");
            break;
    }
}

- (void)centralManagerDidUpdateState:(CBCentralManager *)central
{
    [self bluetoothState:central];
}

@end

//Unity Calls
extern "C" void GetBluetoothState (const char *callback)
{
    if (callback == NULL || (int)strcmp(callback, "") == 0)
    {
        return;
    }
    
    [[BluetoothInfo sharedInstance] updateBluetoothState:[NSString stringWithUTF8String:callback]];
}

extern "C" float GetVolume()
{
// TODO: depracated
	return [[MPMusicPlayerController applicationMusicPlayer] volume];
}

extern "C" void SetVolume(float volume)
{
// TODO: depracated
	[[MPMusicPlayerController applicationMusicPlayer] setVolume:volume];
}

extern "C" float GetBrightness()
{
    return [[UIScreen mainScreen] brightness];
}

extern "C" void SetBrightness(float brightness)
{
    [[UIScreen mainScreen] setBrightness:brightness];
}

extern "C" bool IsMuted()
{
	CFStringRef state;
	UInt32 propertySize = sizeof(CFStringRef);
// TODO: depracated
	AudioSessionInitialize(NULL, NULL, NULL, NULL);
	AudioSessionGetProperty(kAudioSessionProperty_AudioRoute, &propertySize, &state);
	if (CFStringGetLength(state) > 0)
	{
		return NO;
	}
	else
	{
		return YES;
	}
}

extern "C" long GetFreeSpace()
{
    uint64_t totalSpace = 0;
    uint64_t totalFreeSpace = 0;
    NSError *error = nil;
    NSArray *paths = NSSearchPathForDirectoriesInDomains(NSDocumentDirectory, NSUserDomainMask, YES);
    NSDictionary *dictionary = [[NSFileManager defaultManager] attributesOfFileSystemForPath:[paths lastObject] error:&error];
    
    if (dictionary)
    {
        NSNumber *fileSystemSizeInBytes = [dictionary objectForKey: NSFileSystemSize];
        NSNumber *freeFileSystemSizeInBytes = [dictionary objectForKey:NSFileSystemFreeSize];
        totalSpace = [fileSystemSizeInBytes unsignedLongLongValue];
        totalFreeSpace = [freeFileSystemSizeInBytes unsignedLongLongValue];
    }
    else
    {
        NSLog(@"Error Obtaining System Memory Info: Domain = %@, Code = %ld", [error domain], (long)[error code]);
    }
    
    return totalFreeSpace;
}

//get totaly memory
extern "C" unsigned long long GetPhysicalMemory()
{
    unsigned long long mem = [NSProcessInfo processInfo].physicalMemory;
    NSLog(@"physical memory %llu", mem);
    return mem;
}

// Get Used Memory
extern "C" long GetAvailableMemory(void)
{
    struct task_basic_info info;
    mach_msg_type_number_t size = sizeof(info);
    kern_return_t kerr = task_info(mach_task_self(),
                                   TASK_BASIC_INFO,
                                   (task_info_t)&info,
                                   &size);
    if( kerr == KERN_SUCCESS ) {
        NSLog(@"Memory in use (in bytes): %lu", info.resident_size);
        NSLog(@"Memory in use (in MB): %f", ((CGFloat)info.resident_size / 1000000));
        return info.virtual_size;
    } else {
        return 111111;
    }
}

extern "C" float GetBatteryRemaining()
{
    UIDevice *myDevice = [UIDevice currentDevice];
    [myDevice setBatteryMonitoringEnabled:YES];
    return (float)[myDevice batteryLevel];
}

extern "C" bool IsDebuggerAttached()
{
    int                 junk;
    int                 mib[4];
    struct kinfo_proc   info;
    size_t              size;
    
    // Initialize the flags so that, if sysctl fails for some bizarre
    // reason, we get a predictable result.
    info.kp_proc.p_flag = 0;
    
    // Initialize mib, which tells sysctl the info we want, in this case
    // we're looking for information about a specific process ID.
    mib[0] = CTL_KERN;
    mib[1] = KERN_PROC;
    mib[2] = KERN_PROC_PID;
    mib[3] = getpid();
    
    // Call sysctl.
    size = sizeof(info);
    junk = sysctl(mib, sizeof(mib) / sizeof(*mib), &info, &size, NULL, 0);
    assert(junk == 0);
    
    // We're being debugged if the P_TRACED flag is set.
    return ( (info.kp_proc.p_flag & P_TRACED) != 0 );
}






