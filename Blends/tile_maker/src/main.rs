use std::{
    fs::File,
    io::{BufReader, BufWriter},
    path::Path,
};

#[cfg(test)]
pub mod fill_test;

fn main() {
    let decoder = png::Decoder::new(BufReader::new(
        File::open("AncientDomeAlbedo_WIP.png").unwrap(),
    ));
    let mut reader = decoder.read_info().unwrap();
    let mut buf = vec![0; reader.output_buffer_size().unwrap()];
    let info = reader.next_frame(&mut buf).unwrap();
    let bytes = &buf[..info.buffer_size()];

    let w = 4096;
    let h = 4096;

    let mut img = Image {
        w,
        h,
        data: bytes.to_vec(),
    };

    visit_image(&mut img);

    let path = Path::new(r"AncientDomeAlbedo_Processed.png");
    let file = File::create(path).unwrap();
    let writer = BufWriter::new(file);

    let mut encoder = png::Encoder::new(writer, w as u32, h as u32);
    encoder.set_color(png::ColorType::Rgba);
    encoder.set_depth(png::BitDepth::Eight);
    encoder.set_source_gamma(png::ScaledFloat::from_scaled(45455));
    encoder.set_source_gamma(png::ScaledFloat::new(1.0 / 2.2));
    let source_chromaticities = png::SourceChromaticities::new(
        (0.31270, 0.32900),
        (0.64000, 0.33000),
        (0.30000, 0.60000),
        (0.15000, 0.06000),
    );
    encoder.set_source_chromaticities(source_chromaticities);
    let mut writer = encoder.write_header().unwrap();

    let data = &img.data.as_ref();
    writer.write_image_data(data).unwrap(); //
}

pub fn visit_image(image: &mut Image) {
    let (w, h) = (image.width(), image.height());
    let mut visited = Matrix::new(w, h, 0_u8);

    for x in 0..w {
        for y in 0..h {
            if visited.get(x, y) != 0 {
                continue;
            }

            let pixel = image.get_pixel(x, y);

            if pixel.a < 10 {
                continue;
            }

            // if pixel.r == 0 && pixel.g == 0 && pixel.b == 0 {
            //     continue;
            // }

            visit_zone(image, &mut visited, x, y);
        }
    }
}

fn visit_zone(image: &mut Image, visited: &mut Matrix<u8>, x: usize, y: usize) {
    let (color, count) = calc_color(image, visited, x as isize, y as isize);
    let avg_color = Color {
        r: (color.r as f64 / count as f64) as usize,
        g: (color.g as f64 / count as f64) as usize,
        b: (color.b as f64 / count as f64) as usize,
        a: 255,
    };
    set_color(image, visited, x as isize, y as isize, avg_color);
}

fn calc_color(image: &Image, visited: &mut Matrix<u8>, x: isize, y: isize) -> (Color, usize) {
    let mut stack = Vec::new();
    stack.push((x, y));

    let mut color = Color::gray(0);
    let mut count = 0;

    while let Some((x, y)) = stack.pop() {
        if x < 0 || y < 0 || x as usize >= image.width() || y as usize >= image.height() {
            continue;
        }
        let (x, y) = (x as usize, y as usize);

        if visited.get(x, y) != 0 {
            continue;
        }

        let pixel = image.get_pixel(x, y);
        if pixel.a < 10 {
            continue;
        }

        color.r += pixel.r;
        color.g += pixel.g;
        color.b += pixel.b;
        color.a += pixel.a;
        count += 1;

        visited.set(x, y, 1);
        for (dx, dy) in [(-1, 0), (1, 0), (0, -1), (0, 1)] {
            stack.push((x as isize + dx, y as isize + dy));
        }
    }

    (color, count)
}

fn set_color(image: &mut Image, visited: &mut Matrix<u8>, x: isize, y: isize, color: Color) {
   let mut stack = Vec::new();
    stack.push((x, y));

    while let Some((x, y)) = stack.pop() {
        if x < 0 || y < 0 || x as usize >= image.width() || y as usize >= image.height() {
            continue;
        }
        let (x, y) = (x as usize, y as usize);

        if visited.get(x, y) != 1 {
            continue;
        }

        image.set_pixel(x, y, color);

        visited.set(x, y, 2);
        for (dx, dy) in [(-1, 0), (1, 0), (0, -1), (0, 1)] {
            stack.push((x as isize + dx, y as isize + dy));
        }
    }
}

#[derive(Clone, Copy, Debug)]
pub struct Color {
    pub r: usize,
    pub g: usize,
    pub b: usize,
    pub a: usize,
}

impl Color {
    pub fn gray(c: usize) -> Self {
        Self {
            r: c,
            g: c,
            b: c,
            a: c,
        }
    }
}

pub struct Image {
    w: usize,
    h: usize,
    data: Vec<u8>,
}

impl Image {
    pub fn get_pixel(&self, x: usize, y: usize) -> Color {
        let i = (self.w * y + x) * 4;
        let d = &self.data[i..i + 4];
        Color {
            r: d[0] as usize,
            g: d[1] as usize,
            b: d[2] as usize,
            a: d[3] as usize,
        }
    }

    pub fn set_pixel(&mut self, x: usize, y: usize, color: Color) {
        let i = (self.w * y + x)*4;
        let d = &mut self.data[i..i + 4];
        d[0] = color.r as u8;
        d[1] = color.g as u8;
        d[2] = color.b as u8;
        d[3] = color.a as u8;
    }

    pub fn width(&self) -> usize {
        self.w
    }
    pub fn height(&self) -> usize {
        self.h
    }
}

pub struct Matrix<T> {
    w: usize,
    h: usize,
    data: Vec<T>,
}

impl<T: Copy> Matrix<T> {
    pub fn new(w: usize, h: usize, t: T) -> Self {
        Self {
            w,
            h,
            data: vec![t; w * h],
        }
    }

    pub fn get(&self, x: usize, y: usize) -> T {
        let i = self.w * y + x;
        self.data[i]
    }

    pub fn set(&mut self, x: usize, y: usize, t: T) {
        let i = self.w * y + x;
        self.data[i] = t
    }

    pub fn width(&self) -> usize {
        self.w
    }
    pub fn height(&self) -> usize {
        self.h
    }
}
