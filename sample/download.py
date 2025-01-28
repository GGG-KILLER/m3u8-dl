#! /usr/bin/env nix-shell
#! nix-shell -i python3 -p 'python313.withPackages (p: with p; [requests])'
import sys
import os
import requests
import re

HEADERS = {
  'Accept': '*/*',
  'Accept-Language': 'en-US,en;q=0.9',
  'User-Agent': 'Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/130.0.0.0 Safari/537.36'
}


def downloadM3u8(m3u8url, headers=HEADERS, depth=0):
    """ recursively download m3u8 files"""
    base_url = '/'.join(m3u8url.split('/')[0:-1]) + '/' # get the base url
    print('base url: {}'.format(base_url))
    print('processing: {}'.format(m3u8url))
    m3u8 = requests.get(m3u8url, headers=HEADERS) # get the m3u8 file
    folder = m3u8url.split('/')[-2] # get the filename
    parent_folder = None
    if depth > 0:
        parent_folder = m3u8url.split('/')[-3]
    filename = m3u8url.split('/')[-1].split('?')[0] # get the filename
    path_parts = list(filter(lambda x : x is not None, [
        parent_folder,
        folder,
    ]))

    target_path = os.path.join(*path_parts, filename)

    if not os.path.isdir(os.path.join(*path_parts)):
        os.mkdir(os.path.join(*path_parts))
    with open(target_path, 'wb') as f:
        print('writing file to {}'.format(target_path))
        f.write(m3u8.content)


    # Download encrypted key files
    key_urls = extractKeyUrls(m3u8)
    print('key_urls', key_urls)
    for key_url in key_urls:
        key_filename = key_url.split('/')[-1].split('?')[0]
        key_file = requests.get(base_url + key_url, headers=HEADERS)
        with open(os.path.join(*path_parts, key_filename), 'wb') as f:
            f.write(key_file.content)

    ts_urls = extractTsUrls(m3u8) # get all the .ts urls
    print('ts_urls', ts_urls)
    # list the .ts files if they exist in the dir
    # list contents of the directory
    ts_target_dir = os.path.join(*path_parts)
    ts_files = set(filter(lambda x: '.ts' in x, os.listdir(ts_target_dir)))
    print('all ts files existing: {}'.format(ts_files))
    if len(ts_files) > 0:
        ts_urls = list(filter(lambda x: x.split('?')[0] not in ts_files, ts_urls))
    for ts in ts_urls:
        ts_url = base_url + ts
        print('downloading: {}'.format(ts_url))
        ts_filename = ts.split('?')[0]
        ts_file = requests.get(ts_url, headers=HEADERS)
        with open(os.path.join(*path_parts, ts_filename), 'wb') as f:
            f.write(ts_file.content)
    child_urls = extractM3u8Urls(m3u8) # get all the urls in the m3u8 file
    all_urls = []
    print('child_urls', child_urls)
    for child in child_urls:
        new_url = base_url + child
        all_urls.append(new_url)
        subchildren = downloadM3u8(new_url, headers=HEADERS, depth=depth + 1)
        print('subchildren', subchildren)
        all_urls.extend(subchildren)
    return all_urls

def extractTsUrls(m3):
    """ get a list of .ts urls from the m3u8 file """
    lines = m3.text.split('\n')
    urls = []
    for line in lines:
        if '.ts' in line:
            urls.append(line.strip())
    return urls

def extractM3u8Urls(m3):
    """ get a list of m3u8 urls from the m3u8 file """
    lines = m3.text.split('\n')
    urls = []
    for line in lines:
        if '.m3u8' in line:
            urls.append(line.strip())
    return urls


def extractKeyUrls(m3):
  """ get a list of key urls from the m3u8 file """
  lines = m3.text.split('\n')
  urls = []
  for line in lines:
    match = re.search(r'URI="([^"]+)"', line)
    if match:
      urls.append(match.group(1))
  return urls

downloadM3u8("https://sample.vodobox.net/skate_phantom_flex_4k/skate_phantom_flex_4k.m3u8", headers=HEADERS)
print('done')
