import tensorflow_hub as hub
import tensorflow as tf
import numpy as np
import uuid
import os
import cv2

model = hub.load('https://tfhub.dev/google/magenta/arbitrary-image-stylization-v1-256/2')

def load_image(img_path):
    img = tf.io.read_file(img_path)
    img = tf.image.decode_image(img, channels=3)
    img = tf.image.convert_image_dtype(img, tf.float32)
    img = tf.image.resize(img, (256, 256))
    img = img[tf.newaxis, :]
    return img

def run_style_transfer(content_path, style_path, alpha=1.0):
    content_image = load_image(content_path)
    style_image = load_image(style_path)

    stylized_image = model(tf.constant(content_image), tf.constant(style_image))[0]

    alpha = max(0.0, min(alpha, 1.0))

    blended_image = (1 - alpha) * content_image + alpha * stylized_image

    output_image = np.squeeze(blended_image.numpy()) * 255
    output_image = output_image.astype(np.uint8)

    output_dir = 'static'
    os.makedirs(output_dir, exist_ok=True)

    output_filename = f"output_{uuid.uuid4().hex[:8]}.jpg"
    output_path = os.path.join(output_dir, output_filename)
    cv2.imwrite(output_path, cv2.cvtColor(output_image, cv2.COLOR_RGB2BGR))

    return output_path
