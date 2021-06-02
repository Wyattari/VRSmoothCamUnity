#pragma once

#ifdef _WIN32
#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN
#endif // !WIN32_LEAN_AND_MEAN
#include <d3d11.h>
#include <d3d12.h>
#include <windows.h>
#endif

#include <vulkan/vulkan.h>

struct IUnityXRTrace;
extern IUnityXRTrace* s_Trace;

#define DEBUG_TRACE 1

#if DEBUG_TRACE
#define TRACE(STRING, ...) s_Trace->Trace(kXRLogTypeDebug, "[Mock] " STRING, ##__VA_ARGS__)
#else
#define TRACE(STRING, ...)
#endif

#define XR_NO_PROTOTYPES
#include "openxr/openxr.h"
#include "openxr/openxr_platform.h"
#include <openxr/openxr_reflection.h>

#include "XR/IUnityXRTrace.h"

#include <chrono>
#include <map>
#include <queue>
#include <string>
#include <unordered_map>

#include "openxr/openxr_reflection.h"

#include "enums_to_string.h"
#include "openxr_utils.h"

class MockRuntime;
extern MockRuntime* s_runtime;

#define GET_PROC_ADDRESS(funcName)                 \
    if (strcmp(#funcName, name) == 0)              \
    {                                              \
        *function = (PFN_xrVoidFunction)&funcName; \
        return XR_SUCCESS;                         \
    }

#define GET_PROC_ADDRESS_REMAP(funcName, funcProc) \
    if (strcmp(#funcName, name) == 0)              \
    {                                              \
        *function = (PFN_xrVoidFunction)&funcProc; \
        return XR_SUCCESS;                         \
    }

#define CHECK_RUNTIME()       \
    if (s_runtime == nullptr) \
        return XR_ERROR_HANDLE_INVALID;
#define CHECK_INSTANCE(instance)                                      \
    if (s_runtime == nullptr || s_runtime->GetInstance() != instance) \
        return XR_ERROR_HANDLE_INVALID;
#define CHECK_SESSION(session)                                      \
    if (s_runtime == nullptr || s_runtime->GetSession() != session) \
        return XR_ERROR_HANDLE_INVALID;
#define CHECK_SUCCESS(body)       \
    {                             \
        XrResult result = (body); \
        if (result != XR_SUCCESS) \
            return result;        \
    }

#define DEBUG_LOG_EVERY_FUNC_CALL 0

#if DEBUG_LOG_EVERY_FUNC_CALL
#define LOG_FUNC() s_Trace->Trace(kXRLogTypeDebug, "[Mock] %s\n", __FUNCTION__)
#else
#define LOG_FUNC()
#endif

#include "openxr_mock_driver.h"

#include "mock_extensions.h"
#include "mock_input_state.h"
#include "mock_runtime.h"
