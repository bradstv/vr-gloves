// Copyright (c) 2023 LucidVR
//
// SPDX-License-Identifier: MIT
//
// Initial Author: danwillm

#pragma once

#include <functional>
#include <memory>

#include "opengloves_interface.h"

struct HapticFeedbackData {
  bool thumb;
  bool index;
  bool middle;
  bool ring;
  bool pinky;
};

class InputHapticFeedbackNamedPipe {
 public:
  InputHapticFeedbackNamedPipe(og::Hand hand, std::function<void(const HapticFeedbackData&)> on_data_callback);

  void StartListener();

  ~InputHapticFeedbackNamedPipe();
 private:
  class Impl;
  std::unique_ptr<Impl> pImpl_;
};