// Copyright (c) 2023 LucidVR
//
// SPDX-License-Identifier: MIT
//
// Initial Author: danwillm

#pragma once

#include <functional>
#include <memory>

#include "opengloves_interface.h"

struct ThermoFeedbackData {
  short value;
};

class InputThermoFeedbackNamedPipe {
 public:
  InputThermoFeedbackNamedPipe(og::Hand hand, std::function<void(const ThermoFeedbackData&)> on_data_callback);

  void StartListener();

  ~InputThermoFeedbackNamedPipe();
 private:
  class Impl;
  std::unique_ptr<Impl> pImpl_;
};