#include "mock.h"

struct MockInputSourcePath
{
    const char* path;
    XrActionType type;
};

struct MockInteractionProfileDef
{
    const char* name;
    std::vector<const char*> userPaths;
    std::vector<MockInputSourcePath> inputSources;
};

static std::vector<MockInteractionProfileDef> s_InteractionProfiles = {
    // KHR Simple controller
    {
        "/interaction_profiles/khr/simple_controller",
        {"/user/hand/left",
            "/user/hand/right"},
        {{"/user/hand/left/input/select/click", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/left/input/menu/click", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/left/input/grip/pose", XR_ACTION_TYPE_POSE_INPUT},
            {"/user/hand/left/input/aim/pose", XR_ACTION_TYPE_POSE_INPUT},
            {"/user/hand/left/output/haptic", XR_ACTION_TYPE_VIBRATION_OUTPUT},
            {"/user/hand/right/input/select/click", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/right/input/menu/click", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/right/input/grip/pose", XR_ACTION_TYPE_POSE_INPUT},
            {"/user/hand/right/input/aim/pose", XR_ACTION_TYPE_POSE_INPUT},
            {"/user/hand/right/output/haptic", XR_ACTION_TYPE_VIBRATION_OUTPUT}}},

    // Microsoft Mixed Reality Motion Controller Profile
    {
        "/interaction_profiles/microsoft/motion_controller",
        {"/user/hand/left",
            "/user/hand/right"},
        {{"/user/hand/left/input/menu/click", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/left/input/squeeze/click", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/left/input/trigger/value", XR_ACTION_TYPE_FLOAT_INPUT},
            {"/user/hand/left/input/thumbstick/x", XR_ACTION_TYPE_FLOAT_INPUT},
            {"/user/hand/left/input/thumbstick/y", XR_ACTION_TYPE_FLOAT_INPUT},
            {"/user/hand/left/input/thumbstick/click", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/left/input/thumbstick", XR_ACTION_TYPE_VECTOR2F_INPUT},
            {"/user/hand/left/input/trackpad/x", XR_ACTION_TYPE_FLOAT_INPUT},
            {"/user/hand/left/input/trackpad/y", XR_ACTION_TYPE_FLOAT_INPUT},
            {"/user/hand/left/input/trackpad", XR_ACTION_TYPE_VECTOR2F_INPUT},
            {"/user/hand/left/input/trackpad/click", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/left/input/trackpad/touch", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/left/input/grip/pose", XR_ACTION_TYPE_POSE_INPUT},
            {"/user/hand/left/input/aim/pose", XR_ACTION_TYPE_POSE_INPUT},
            {"/user/hand/left/output/haptic", XR_ACTION_TYPE_VIBRATION_OUTPUT},
            {"/user/hand/right/input/menu/click", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/right/input/squeeze/click", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/right/input/trigger/value", XR_ACTION_TYPE_FLOAT_INPUT},
            {"/user/hand/right/input/thumbstick/x", XR_ACTION_TYPE_FLOAT_INPUT},
            {"/user/hand/right/input/thumbstick/y", XR_ACTION_TYPE_FLOAT_INPUT},
            {"/user/hand/right/input/thumbstick/click", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/right/input/thumbstick", XR_ACTION_TYPE_VECTOR2F_INPUT},
            {"/user/hand/right/input/trackpad/x", XR_ACTION_TYPE_FLOAT_INPUT},
            {"/user/hand/right/input/trackpad/y", XR_ACTION_TYPE_FLOAT_INPUT},
            {"/user/hand/right/input/trackpad", XR_ACTION_TYPE_VECTOR2F_INPUT},
            {"/user/hand/right/input/trackpad/click", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/right/input/trackpad/touch", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/right/input/grip/pose", XR_ACTION_TYPE_POSE_INPUT},
            {"/user/hand/right/input/aim/pose", XR_ACTION_TYPE_POSE_INPUT},
            {"/user/hand/right/output/haptic", XR_ACTION_TYPE_VIBRATION_OUTPUT}}},

    // Google Daydream Controller Profile
    {
        "/interaction_profiles/google/daydream_controller",
        {"/user/hand/left",
            "/user/hand/right"},
        {{"/user/hand/left/input/select/click", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/left/input/trackpad/x", XR_ACTION_TYPE_FLOAT_INPUT},
            {"/user/hand/left/input/trackpad/y", XR_ACTION_TYPE_FLOAT_INPUT},
            {"/user/hand/left/input/trackpad/click", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/left/input/trackpad/touch", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/left/input/trackpad", XR_ACTION_TYPE_VECTOR2F_INPUT},
            {"/user/hand/left/input/grip/pose", XR_ACTION_TYPE_POSE_INPUT},
            {"/user/hand/left/input/aim/pose", XR_ACTION_TYPE_POSE_INPUT},
            {"/user/hand/right/input/select/click", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/right/input/trackpad/x", XR_ACTION_TYPE_FLOAT_INPUT},
            {"/user/hand/right/input/trackpad/y", XR_ACTION_TYPE_FLOAT_INPUT},
            {"/user/hand/right/input/trackpad/click", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/right/input/trackpad/touch", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/right/input/trackpad", XR_ACTION_TYPE_VECTOR2F_INPUT},
            {"/user/hand/right/input/grip/pose", XR_ACTION_TYPE_POSE_INPUT},
            {"/user/hand/right/input/aim/pose", XR_ACTION_TYPE_POSE_INPUT}}},

    // HTC Vive Controller Profile
    {
        "/interaction_profiles/htc/vive_controller",
        {"/user/hand/left",
            "/user/hand/right"},
        {{"/user/hand/left/input/menu/click", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/left/input/squeeze/click", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/left/input/trigger/value", XR_ACTION_TYPE_FLOAT_INPUT},
            {"/user/hand/left/input/trigger/click", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/left/input/trackpad/x", XR_ACTION_TYPE_FLOAT_INPUT},
            {"/user/hand/left/input/trackpad/y", XR_ACTION_TYPE_FLOAT_INPUT},
            {"/user/hand/left/input/trackpad/click", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/left/input/trackpad/touch", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/left/input/trackpad", XR_ACTION_TYPE_VECTOR2F_INPUT},
            {"/user/hand/left/input/grip/pose", XR_ACTION_TYPE_POSE_INPUT},
            {"/user/hand/left/input/aim/pose", XR_ACTION_TYPE_POSE_INPUT},
            {"/user/hand/left/output/haptic", XR_ACTION_TYPE_VIBRATION_OUTPUT},
            {"/user/hand/right/input/menu/click", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/right/input/squeeze/click", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/right/input/trigger/value", XR_ACTION_TYPE_FLOAT_INPUT},
            {"/user/hand/right/input/trigger/click", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/right/input/trackpad/x", XR_ACTION_TYPE_FLOAT_INPUT},
            {"/user/hand/right/input/trackpad/y", XR_ACTION_TYPE_FLOAT_INPUT},
            {"/user/hand/right/input/trackpad/click", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/right/input/trackpad/touch", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/right/input/trackpad", XR_ACTION_TYPE_VECTOR2F_INPUT},
            {"/user/hand/right/input/grip/pose", XR_ACTION_TYPE_POSE_INPUT},
            {"/user/hand/right/input/aim/pose", XR_ACTION_TYPE_POSE_INPUT},
            {"/user/hand/right/output/haptic", XR_ACTION_TYPE_VIBRATION_OUTPUT}}},

    // HTC Vive Pro Profile
    {
        "/interaction_profiles/htc/vive_pro",
        {"/user/head"},
        {
            {"/user/head/input/volume_up/click", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/head/input/volume_down/click", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/head/input/mute_mic/click", XR_ACTION_TYPE_BOOLEAN_INPUT},
        }},

    // Microsoft Xbox Controller Profile
    {
        "/interaction_profiles/microsoft/xbox_controller",
        {"/user/gamepad"},
        {{"/user/gamepad/input/menu/click", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/gamepad/input/view/click", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/gamepad/input/a/click", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/gamepad/input/b/click", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/gamepad/input/x/click", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/gamepad/input/y/click", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/gamepad/input/dpad_down/click", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/gamepad/input/dpad_right/click", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/gamepad/input/dpad_up/click", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/gamepad/input/dpad_left/click", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/gamepad/input/shoulder_left/click", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/gamepad/input/shoulder_right/click", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/gamepad/input/thumbstick_left/click", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/gamepad/input/thumbstick_right/click", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/gamepad/input/trigger_left/value", XR_ACTION_TYPE_FLOAT_INPUT},
            {"/user/gamepad/input/trigger_right/value", XR_ACTION_TYPE_FLOAT_INPUT},
            {"/user/gamepad/input/thumbstick_left/x", XR_ACTION_TYPE_FLOAT_INPUT},
            {"/user/gamepad/input/thumbstick_left/y", XR_ACTION_TYPE_FLOAT_INPUT},
            {"/user/gamepad/input/thumbstick_left", XR_ACTION_TYPE_VECTOR2F_INPUT},
            {"/user/gamepad/input/thumbstick_right/x", XR_ACTION_TYPE_FLOAT_INPUT},
            {"/user/gamepad/input/thumbstick_right/y", XR_ACTION_TYPE_FLOAT_INPUT},
            {"/user/gamepad/input/thumbstick_right", XR_ACTION_TYPE_VECTOR2F_INPUT},
            {"/user/gamepad/output/haptic_left", XR_ACTION_TYPE_VIBRATION_OUTPUT},
            {"/user/gamepad/output/haptic_right", XR_ACTION_TYPE_VIBRATION_OUTPUT},
            {"/user/gamepad/output/haptic_left_trigger", XR_ACTION_TYPE_VIBRATION_OUTPUT},
            {"/user/gamepad/output/haptic_right_trigger", XR_ACTION_TYPE_VIBRATION_OUTPUT}}},

    // Oculus Go Controller Profile
    {
        "/interaction_profiles/oculus/go_controller",
        {"/user/hand/left",
            "/user/hand/right"},
        {
            {"/user/hand/left/input/trigger/click", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/left/input/back/click", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/left/input/trackpad/x", XR_ACTION_TYPE_FLOAT_INPUT},
            {"/user/hand/left/input/trackpad/y", XR_ACTION_TYPE_FLOAT_INPUT},
            {"/user/hand/left/input/trackpad", XR_ACTION_TYPE_VECTOR2F_INPUT},
            {"/user/hand/left/input/trackpad/click", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/left/input/trackpad/touch", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/left/input/grip/pose", XR_ACTION_TYPE_POSE_INPUT},
            {"/user/hand/left/input/aim/pose", XR_ACTION_TYPE_POSE_INPUT},
            {"/user/hand/right/input/trigger/click", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/right/input/back/click", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/right/input/trackpad/x", XR_ACTION_TYPE_FLOAT_INPUT},
            {"/user/hand/right/input/trackpad/y", XR_ACTION_TYPE_FLOAT_INPUT},
            {"/user/hand/right/input/trackpad", XR_ACTION_TYPE_VECTOR2F_INPUT},
            {"/user/hand/right/input/trackpad/click", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/right/input/trackpad/touch", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/right/input/grip/pose", XR_ACTION_TYPE_POSE_INPUT},
            {"/user/hand/right/input/aim/pose", XR_ACTION_TYPE_POSE_INPUT},
        }},

    // Oculus Touch Controller Profile
    {
        "/interaction_profiles/oculus/touch_controller",
        {"/user/hand/left",
            "/user/hand/right"},
        {
            {"/user/hand/left/input/x/click", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/left/input/x/touch", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/left/input/y/click", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/left/input/y/touch", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/left/input/menu/click", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/left/input/squeeze/value", XR_ACTION_TYPE_FLOAT_INPUT},
            {"/user/hand/left/input/trigger/value", XR_ACTION_TYPE_FLOAT_INPUT},
            {"/user/hand/left/input/trigger/touch", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/left/input/thumbstick/x", XR_ACTION_TYPE_FLOAT_INPUT},
            {"/user/hand/left/input/thumbstick/y", XR_ACTION_TYPE_FLOAT_INPUT},
            {"/user/hand/left/input/thumbstick/click", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/left/input/thumbstick/touch", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/left/input/thumbstick", XR_ACTION_TYPE_VECTOR2F_INPUT},
            // Rift S and Quest controllers lack thumbrests
            // {"/user/hand/left/input/thumbrest/touch", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/left/input/grip/pose", XR_ACTION_TYPE_POSE_INPUT},
            {"/user/hand/left/input/aim/pose", XR_ACTION_TYPE_POSE_INPUT},
            {"/user/hand/left/output/haptic", XR_ACTION_TYPE_VIBRATION_OUTPUT},
            {"/user/hand/right/input/a/click", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/right/input/a/touch", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/right/input/b/click", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/right/input/b/touch", XR_ACTION_TYPE_BOOLEAN_INPUT},
            // The system ("Oculus") button is reserved for system applications
            // {"/user/hand/right/input/system/click", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/right/input/squeeze/value", XR_ACTION_TYPE_FLOAT_INPUT},
            {"/user/hand/right/input/trigger/value", XR_ACTION_TYPE_FLOAT_INPUT},
            {"/user/hand/right/input/trigger/touch", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/right/input/thumbstick/x", XR_ACTION_TYPE_FLOAT_INPUT},
            {"/user/hand/right/input/thumbstick/y", XR_ACTION_TYPE_FLOAT_INPUT},
            {"/user/hand/right/input/thumbstick/click", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/right/input/thumbstick/touch", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/right/input/thumbstick", XR_ACTION_TYPE_VECTOR2F_INPUT},
            // Rift S and Quest controllers lack thumbrests
            // {"/user/hand/right/input/thumbrest/touch", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/right/input/grip/pose", XR_ACTION_TYPE_POSE_INPUT},
            {"/user/hand/right/input/aim/pose", XR_ACTION_TYPE_POSE_INPUT},
            {"/user/hand/right/output/haptic", XR_ACTION_TYPE_VIBRATION_OUTPUT},
        }},

    // Valve Index Controller Profile
    {
        "/interaction_profiles/valve/index_controller",
        {"/user/hand/left",
            "/user/hand/right"},
        {
            {"/user/hand/left/input/a/click", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/left/input/a/touch", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/left/input/b/click", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/left/input/b/touch", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/left/input/squeeze/value", XR_ACTION_TYPE_FLOAT_INPUT},
            {"/user/hand/left/input/squeeze/force", XR_ACTION_TYPE_FLOAT_INPUT},
            {"/user/hand/left/input/trigger/click", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/left/input/trigger/value", XR_ACTION_TYPE_FLOAT_INPUT},
            {"/user/hand/left/input/trigger/touch", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/left/input/thumbstick/x", XR_ACTION_TYPE_FLOAT_INPUT},
            {"/user/hand/left/input/thumbstick/y", XR_ACTION_TYPE_FLOAT_INPUT},
            {"/user/hand/left/input/thumbstick/click", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/left/input/thumbstick/touch", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/left/input/thumbstick", XR_ACTION_TYPE_VECTOR2F_INPUT},
            {"/user/hand/left/input/trackpad/x", XR_ACTION_TYPE_FLOAT_INPUT},
            {"/user/hand/left/input/trackpad/y", XR_ACTION_TYPE_FLOAT_INPUT},
            {"/user/hand/left/input/trackpad/force", XR_ACTION_TYPE_FLOAT_INPUT},
            {"/user/hand/left/input/trackpad/touch", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/left/input/trackpad", XR_ACTION_TYPE_VECTOR2F_INPUT},
            {"/user/hand/left/input/grip/pose", XR_ACTION_TYPE_POSE_INPUT},
            {"/user/hand/left/input/aim/pose", XR_ACTION_TYPE_POSE_INPUT},
            {"/user/hand/left/output/haptic", XR_ACTION_TYPE_VIBRATION_OUTPUT},
            {"/user/hand/right/input/a/click", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/right/input/a/touch", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/right/input/b/click", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/right/input/b/touch", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/right/input/squeeze/value", XR_ACTION_TYPE_FLOAT_INPUT},
            {"/user/hand/right/input/squeeze/force", XR_ACTION_TYPE_FLOAT_INPUT},
            {"/user/hand/right/input/trigger/click", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/right/input/trigger/value", XR_ACTION_TYPE_FLOAT_INPUT},
            {"/user/hand/right/input/trigger/touch", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/right/input/thumbstick/x", XR_ACTION_TYPE_FLOAT_INPUT},
            {"/user/hand/right/input/thumbstick/y", XR_ACTION_TYPE_FLOAT_INPUT},
            {"/user/hand/right/input/thumbstick/click", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/right/input/thumbstick/touch", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/right/input/thumbstick", XR_ACTION_TYPE_VECTOR2F_INPUT},
            {"/user/hand/right/input/trackpad/x", XR_ACTION_TYPE_FLOAT_INPUT},
            {"/user/hand/right/input/trackpad/y", XR_ACTION_TYPE_FLOAT_INPUT},
            {"/user/hand/right/input/trackpad/force", XR_ACTION_TYPE_FLOAT_INPUT},
            {"/user/hand/right/input/trackpad/touch", XR_ACTION_TYPE_BOOLEAN_INPUT},
            {"/user/hand/right/input/trackpad", XR_ACTION_TYPE_VECTOR2F_INPUT},
            {"/user/hand/right/input/grip/pose", XR_ACTION_TYPE_POSE_INPUT},
            {"/user/hand/right/input/aim/pose", XR_ACTION_TYPE_POSE_INPUT},
            {"/user/hand/right/output/haptic", XR_ACTION_TYPE_VIBRATION_OUTPUT},
        }},
};

void MockRuntime::InitializeInteractionProfiles()
{
    interactionProfiles.reserve(s_InteractionProfiles.size());
    for (MockInteractionProfileDef& def : s_InteractionProfiles)
    {
        interactionProfiles.emplace_back();
        MockInteractionProfile& mockProfile = interactionProfiles.back();
        mockProfile.path = StringToPath(def.name);
        mockProfile.userPaths.reserve(def.userPaths.size());
        for (const char* userPathString : def.userPaths)
            mockProfile.userPaths.push_back(StringToPath(userPathString));

        mockProfile.inputSources.reserve(def.inputSources.size());
        for (MockInputSourcePath& componentDef : def.inputSources)
        {
            mockProfile.inputSources.push_back({StringToPath(componentDef.path), componentDef.type});
        }
    }

    // Create action states for each interaction profile
    inputStates.clear();

    for (MockInteractionProfile& mockProfile : interactionProfiles)
    {
        inputStates.reserve(mockProfile.inputSources.size());
        for (auto& inputSource : mockProfile.inputSources)
        {
            AddMockInputState(mockProfile.path, inputSource.path, inputSource.actionType);
        }
    }
}

const MockRuntime::MockInteractionProfile* MockRuntime::GetMockInteractionProfile(XrPath interactionProfile) const
{
    for (const MockInteractionProfile& mockProfile : interactionProfiles)
        if (mockProfile.path == interactionProfile)
            return &mockProfile;

    return nullptr;
}
