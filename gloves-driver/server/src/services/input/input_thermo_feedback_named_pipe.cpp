// Copyright (c) 2023 LucidVR
//
// SPDX-License-Identifier: MIT
//
// Initial Author: danwillm

#include "input_thermo_feedback_named_pipe.h"

#include "named_pipe/named_pipe_win.h"

static og::Logger& logger = og::Logger::GetInstance();

class InputThermoFeedbackNamedPipe::Impl {
 public:
  Impl(og::Hand hand, std::function<void(const ThermoFeedbackData&)> on_data_callback) : on_data_callback_(std::move(on_data_callback)) {
    const std::string pipe_name = R"(\\.\pipe\vrapplication\ffb\thermo\)" + std::string(hand == og::kHandLeft ? "left" : "right");

    pipe_listener_ = std::make_unique<NamedPipeListener<ThermoFeedbackData>>(
        pipe_name,
        [&](const NamedPipeListenerEvent& event) {
          if (event.type == NamedPipeListenerEventType::ClientConnected)
            logger.Log(og::kLoggerLevel_Info, "Thermo force feedback pipe connected for %s hand", hand == og::kHandLeft ? "left" : "right");
        },
        [&](const ThermoFeedbackData* data) { on_data_callback_(*data); });
  }

  void StartListening() {
    pipe_listener_->StartListening();
    logger.Log(og::kLoggerLevel_Info, "Thermo feedback pipe listening");
  }

 private:
  std::function<void(const ThermoFeedbackData&)> on_data_callback_;

  std::unique_ptr<INamedPipeListener> pipe_listener_;
};

InputThermoFeedbackNamedPipe::InputThermoFeedbackNamedPipe(og::Hand hand, std::function<void(const ThermoFeedbackData&)> on_data_callback)
    : pImpl_(std::make_unique<Impl>(hand, std::move(on_data_callback))){};

void InputThermoFeedbackNamedPipe::StartListener() {
  pImpl_->StartListening();
}

InputThermoFeedbackNamedPipe::~InputThermoFeedbackNamedPipe() = default;