import sharp from 'sharp';
import { fileURLToPath } from 'url';
import { dirname, join } from 'path';

const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);

const sizes = [512, 192, 180, 32, 16];
const inputFile = join(__dirname, 'static', 'icon-1024.png');

async function generateIcons() {
	console.log('🎨 Generating icons from icon-1024.png...\n');

	try {
		for (const size of sizes) {
			const outputFile = join(__dirname, 'static', `icon-${size}.png`);
			
			await sharp(inputFile)
				.resize(size, size, {
					kernel: sharp.kernel.lanczos3,
					fit: 'contain',
					background: { r: 0, g: 0, b: 0, alpha: 0 }
				})
				.png({ quality: 100, compressionLevel: 9 })
				.toFile(outputFile);
			
			console.log(`✓ Generated icon-${size}.png (${size}x${size})`);
		}
		
		console.log('\n✨ All icons generated successfully!');
	} catch (error) {
		console.error('❌ Error generating icons:', error.message);
		process.exit(1);
	}
}

generateIcons();
