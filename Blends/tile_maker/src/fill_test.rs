use crate::{Color, Image, Matrix, visit_image};

#[test]
fn test_recursion() {
    let w = 100;
    let h = 100;
    let mut image = Image{ w, h, data: vec![255; w * h * 4] };

    // image.set_pixel(5, 5, Color::gray(255));

    visit_image(&mut image);
}