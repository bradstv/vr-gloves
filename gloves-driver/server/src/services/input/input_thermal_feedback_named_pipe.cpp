// Copyright (c) 2023 LucidVR
//
// SPDX-License-Identifier: MIT
//
// Initial Author: danwillm

#include "input_thermal_feedback_named_pipe.h"

#include "named_pipe/named_pipe_win.h"

static og::Logger& logger = og::Logger::GetInstance();

class InputThermalFeedbackNamedPipe::Impl {
 public:
  Impl(og::Hand hand, std::function<void(const ThermalFeedbackData&)> on_data_callback) : on_data_callback_(std::move(on_data_callback)) {
    const std::string pipe_name = R"(\\.\pipe\vrapplication\ffb\thermal\)" + std::string(hand == og::kHandLeft ? "left" : "right");

    pipe_listener_ = std::make_unique<NamedPipeListener<ThermalFeedbackData>>(
        pipe_name,
        [&](const NamedPipeListenerEvent& event) {
          if (event.type == NamedPipeListenerEventType::ClientConnected)
            logger.Log(og::kLoggerLevel_Info, "Thermal force feedback pipe connected for %s hand", hand == og::kHandLeft ? "left" : "right");
        },
        [&](const ThermalFeedbackData* data) { on_data_callback_(*data); });
  }

  void StartListening() {
    pipe_listener_->StartListening();
    logger.Log(og::kLoggerLevel_Info, "Thermal feedback pipe listening");
  }

 private:
  std::function<void(const ThermalFeedbackData&)> on_data_callback_;

  std::unique_ptr<INamedPipeListener> pipe_listener_;
};

InputThermalFeedbackNamedPipe::InputThermalFeedbackNamedPipe(og::Hand hand, std::function<void(const ThermalFeedbackData&)> on_data_callback)
    : pImpl_(std::make_unique<Impl>(hand, std::move(on_data_callback))){};

void InputThermalFeedbackNamedPipe::StartListener() {
  pImpl_->StartListening();
}

InputThermalFeedbackNamedPipe::~InputThermalFeedbackNamedPipe() = default;