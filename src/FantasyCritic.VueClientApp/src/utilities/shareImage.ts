import { domToBlob } from 'modern-screenshot';

export async function shareElementAsImage(element: HTMLElement, fileName: string, title: string, scale: number): Promise<void> {
  const blob = await domToBlob(element, { scale });
  const file = new File([blob], fileName, { type: blob.type, lastModified: Date.now() });
  const shareData: ShareData = { files: [file], title };

  if (navigator.canShare?.(shareData)) {
    try {
      await navigator.share(shareData);
    } catch (error) {
      if (error instanceof DOMException && error.name === 'AbortError') {
        return;
      }
      throw error;
    }
    return;
  }

  const url = URL.createObjectURL(blob);
  const link = document.createElement('a');
  link.href = url;
  link.download = fileName;
  link.click();
  // Safari can lose the download if the URL is revoked during the click.
  setTimeout(() => URL.revokeObjectURL(url), 60000);
}
