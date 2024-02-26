// Copyright (c) 2023 LucidVR
//
// SPDX-License-Identifier: MIT
//
// Initial Author: danwillm

#pragma once

#include <functional>
#include <memory>

#include "opengloves_interface.h"

struct ThermalFeedbackData {
  short value;
};

class InputThermalFeedbackNamedPipe {
 public:
  InputThermalFeedbackNamedPipe(og::Hand hand, std::function<void(const ThermalFeedbackData&)> on_data_callback);

  void StartListener();

  ~InputThermalFeedbackNamedPipe();
 private:
  class Impl;
  std::unique_ptr<Impl> pImpl_;
};